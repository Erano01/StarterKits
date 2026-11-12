using System;
using System.Collections.Generic;
using System.IO;
using StarterKits.Kits;
using UnityEngine;

namespace StarterKits.Editor
{
    /// <summary>
    /// "Import Kit Image" popup: pick a png/jpg (import folder or a pasted path), crop a square
    /// with mouse drag / wheel / buttons, save it as a 200x200 portrait for the selected kit.
    /// </summary>
    public partial class XUiC_KitEditorWindow
    {
        private const int ImportFileRows = 8;
        private const float ZoomStep = 0.88f;
        private const float MinCropPixels = 24f;

        private XUiController importPopup;
        private XUiV_Label lblImportFolder;
        private XUiV_Label lblImportFilePage;
        private XUiV_Label lblCropHint;
        private XUiV_Label lblCropInfo;
        private XUiV_Label lblImportStatus;
        private XUiV_Texture texCropView;
        private XUiV_Texture texCropPreview;
        private XUiV_Texture texKitImageFile;
        private XUiC_TextInput txtImportPath;
        private XUiC_TextInput txtImportName;
        private readonly XUiC_SimpleButton[] importFileButtons = new XUiC_SimpleButton[ImportFileRows];

        private List<string> importFiles = new List<string>();
        private int importFilePage;
        private Texture2D importSource;

        // Crop square in source texture pixels, origin bottom-left (Unity texture space).
        private float cropX;
        private float cropY;
        private float cropSize;

        private bool ImportOpen => importPopup?.ViewComponent != null && importPopup.ViewComponent.IsVisible;

        private void InitImport()
        {
            importPopup = Find("importPopup");
            lblImportFolder = Label("lblImportFolder");
            lblImportFilePage = Label("lblImportFilePage");
            lblCropHint = Label("lblCropHint");
            lblCropInfo = Label("lblCropInfo");
            lblImportStatus = Label("lblImportStatus");
            texCropView = Find("texCropView")?.ViewComponent as XUiV_Texture;
            texCropPreview = Find("texCropPreview")?.ViewComponent as XUiV_Texture;
            texKitImageFile = Find("texKitImageFile")?.ViewComponent as XUiV_Texture;
            txtImportPath = TextInput("txtImportPath", null);
            txtImportName = TextInput("txtImportName", null);

            if (txtImportPath != null)
            {
                txtImportPath.OnSubmitHandler += (s, text) => LoadImportFile(text);
            }

            for (int i = 0; i < ImportFileRows; i++)
            {
                int row = i;
                importFileButtons[i] = Button("importFile" + i, (s, m) =>
                {
                    int index = importFilePage * ImportFileRows + row;
                    if (index < importFiles.Count)
                    {
                        LoadImportFile(importFiles[index]);
                    }
                });
            }

            Button("btnImportImage", (s, m) => OpenImport());
            Button("btnImportFilePrev", (s, m) => { importFilePage--; RefreshImportFiles(); });
            Button("btnImportFileNext", (s, m) => { importFilePage++; RefreshImportFiles(); });
            Button("btnOpenImportFolder", (s, m) => OpenImportFolder());
            Button("btnRefreshImportFiles", (s, m) => ReloadImportFiles());
            Button("btnLoadImportPath", (s, m) => LoadImportFile(txtImportPath?.Text));

            Button("btnCropZoomIn", (s, m) => ZoomCrop(ZoomStep));
            Button("btnCropZoomOut", (s, m) => ZoomCrop(1f / ZoomStep));
            Button("btnCropLeft", (s, m) => MoveCrop(-1, 0));
            Button("btnCropRight", (s, m) => MoveCrop(1, 0));
            Button("btnCropUp", (s, m) => MoveCrop(0, 1));
            Button("btnCropDown", (s, m) => MoveCrop(0, -1));
            Button("btnCropFit", (s, m) => FitCrop());
            Button("btnCropFill", (s, m) => CenterCrop());

            Button("btnImportCancel", (s, m) => CloseImport());
            Button("btnImportSave", (s, m) => SaveImport());

            XUiController cropView = Find("texCropView");
            if (cropView != null)
            {
                cropView.OnDrag += OnCropDrag;
                cropView.OnScroll += (sender, delta) => ZoomCrop(delta > 0 ? ZoomStep : 1f / ZoomStep);
            }

            SetVisible(importPopup, false);
        }

        private void OpenImport()
        {
            if (SelectedKit == null)
            {
                SetStatus("Select or create a kit first.");
                return;
            }

            ClearImportSource();
            SetInput(txtImportPath, string.Empty);
            SetInput(txtImportName, string.Empty);
            SetLabel(lblImportFolder, KitImageStore.ImportDirectory);
            SetImportStatus("Put pictures in the folder above (Open folder), then click one.");
            ReloadImportFiles();
            SetVisible(importPopup, true);
        }

        private void CloseImport()
        {
            SetVisible(importPopup, false);
            ClearImportSource();
        }

        public override void OnClose()
        {
            base.OnClose();
            if (importPopup != null)
            {
                CloseImport();
            }
        }

        private void ReloadImportFiles()
        {
            importFiles = KitImageStore.ListImportCandidates();
            RefreshImportFiles();
        }

        private void RefreshImportFiles()
        {
            int pages = Math.Max(1, (importFiles.Count + ImportFileRows - 1) / ImportFileRows);
            importFilePage = KitRepository.Clamp(importFilePage, 0, pages - 1);
            SetLabel(lblImportFilePage, importFiles.Count == 0 ? "Folder is empty" : $"{importFilePage + 1} / {pages}  ({importFiles.Count} pictures)");

            for (int i = 0; i < ImportFileRows; i++)
            {
                int index = importFilePage * ImportFileRows + i;
                bool used = index < importFiles.Count;
                SetVisible(importFileButtons[i], used);
                if (used && importFileButtons[i] != null)
                {
                    importFileButtons[i].Text = "  " + Path.GetFileName(importFiles[index]);
                }
            }
        }

        private void OpenImportFolder()
        {
            try
            {
                Directory.CreateDirectory(KitImageStore.ImportDirectory);
                Application.OpenURL(new Uri(KitImageStore.ImportDirectory).AbsoluteUri);
                SetImportStatus("Folder opened. Copy pictures into it, then press Refresh.");
            }
            catch (Exception ex)
            {
                SetImportStatus("Could not open the folder: " + ex.Message);
            }
        }

        private void LoadImportFile(string path)
        {
            path = (path ?? string.Empty).Trim().Trim('"', '\'');
            if (path.StartsWith("~", StringComparison.Ordinal))
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + path.Substring(1);
            }

            if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                path = new Uri(path).LocalPath;
            }

            if (path.Length == 0)
            {
                SetImportStatus("Paste the full path of a .png or .jpg file.");
                return;
            }

            Texture2D texture = KitImageStore.LoadSource(path, out string error);
            if (texture == null)
            {
                SetImportStatus($"Could not open '{Path.GetFileName(path)}': {error}");
                return;
            }

            ClearImportSource();
            importSource = texture;
            FitCrop();
            SetInput(txtImportName, KitImageStore.SanitizeName(Path.GetFileNameWithoutExtension(path)));
            SetImportStatus($"Loaded {Path.GetFileName(path)}. Drag / zoom to frame the portrait, then 'Use for this kit'.");
        }

        private void ClearImportSource()
        {
            if (texCropView != null)
            {
                texCropView.Texture = null;
                texCropView.IsVisible = false;
            }

            if (texCropPreview != null)
            {
                texCropPreview.Texture = null;
                texCropPreview.IsVisible = false;
            }

            if (importSource != null)
            {
                UnityEngine.Object.Destroy(importSource);
                importSource = null;
            }

            if (lblCropHint != null)
            {
                lblCropHint.IsVisible = true;
            }

            SetLabel(lblCropInfo, string.Empty);
        }

        // ---------------------------------------------------------------- crop math

        private float MaxCropSize => importSource == null ? 0f : Mathf.Min(importSource.width, importSource.height);

        /// <summary>Largest square that fits, centered (whole short side visible).</summary>
        private void FitCrop()
        {
            if (importSource == null)
            {
                return;
            }

            cropSize = MaxCropSize;
            CenterCrop();
        }

        private void CenterCrop()
        {
            if (importSource == null)
            {
                return;
            }

            cropX = (importSource.width - cropSize) * 0.5f;
            cropY = (importSource.height - cropSize) * 0.5f;
            ApplyCrop();
        }

        /// <summary>factor &lt; 1 zooms in. Zooms around the current center.</summary>
        private void ZoomCrop(float factor)
        {
            if (importSource == null)
            {
                return;
            }

            float centerX = cropX + cropSize * 0.5f;
            float centerY = cropY + cropSize * 0.5f;
            cropSize = Mathf.Clamp(cropSize * factor, Mathf.Min(MinCropPixels, MaxCropSize), MaxCropSize);
            cropX = centerX - cropSize * 0.5f;
            cropY = centerY - cropSize * 0.5f;
            ApplyCrop();
        }

        /// <summary>Moves the visible picture by 10% of the frame; (1,0) shows more of the right side.</summary>
        private void MoveCrop(int dx, int dy)
        {
            if (importSource == null)
            {
                return;
            }

            cropX += dx * cropSize * 0.1f;
            cropY += dy * cropSize * 0.1f;
            ApplyCrop();
        }

        private void OnCropDrag(XUiController sender, EDragType dragType, Vector2 mouseDelta)
        {
            if (importSource == null || texCropView == null)
            {
                return;
            }

            // Screen pixels -> source pixels: the frame shows cropSize source pixels across its on-screen width.
            float screenWidth = ViewportScreenWidth();
            if (screenWidth <= 1f)
            {
                return;
            }

            float scale = cropSize / screenWidth;

            // Dragging the picture right/up means the crop window moves left/down.
            cropX -= mouseDelta.x * scale;
            cropY -= mouseDelta.y * scale;
            ApplyCrop();
        }

        private float ViewportScreenWidth()
        {
            try
            {
                UITexture widget = texCropView.uiTexture;
                if (widget != null)
                {
                    Camera camera = NGUITools.FindCameraForLayer(widget.gameObject.layer);
                    if (camera != null)
                    {
                        Vector3[] corners = widget.worldCorners;
                        return Mathf.Abs(camera.WorldToScreenPoint(corners[2]).x - camera.WorldToScreenPoint(corners[0]).x);
                    }
                }
            }
            catch
            {
                // Fall through to the estimate below.
            }

            return texCropView.Size.x * Screen.height / 1080f;
        }

        private void ApplyCrop()
        {
            if (importSource == null)
            {
                return;
            }

            float width = importSource.width;
            float height = importSource.height;
            cropSize = Mathf.Clamp(cropSize, Mathf.Min(MinCropPixels, MaxCropSize), MaxCropSize);
            cropX = Mathf.Clamp(cropX, 0f, width - cropSize);
            cropY = Mathf.Clamp(cropY, 0f, height - cropSize);

            var uv = new Rect(cropX / width, cropY / height, cropSize / width, cropSize / height);
            ShowCropTexture(texCropView, uv);
            ShowCropTexture(texCropPreview, uv);

            if (lblCropHint != null)
            {
                lblCropHint.IsVisible = false;
            }

            float zoom = MaxCropSize / cropSize;
            SetLabel(lblCropInfo,
                $"Source: {importSource.width} x {importSource.height}\n" +
                $"Crop: {Mathf.RoundToInt(cropSize)} x {Mathf.RoundToInt(cropSize)} px\n" +
                $"Zoom: {zoom:0.00}x\n" +
                $"Output: {KitImageStore.PortraitSize} x {KitImageStore.PortraitSize}" +
                (cropSize < KitImageStore.PortraitSize ? "\n(upscaled - may look soft)" : string.Empty));
        }

        private void ShowCropTexture(XUiV_Texture view, Rect uv)
        {
            if (view == null)
            {
                return;
            }

            if (view.Texture != importSource)
            {
                view.Texture = importSource;
            }

            view.UVRect = uv;
            view.IsVisible = true;
            view.SetDirty();
        }

        // ---------------------------------------------------------------- save

        private void SaveImport()
        {
            KitDefinition kit = SelectedKit;
            if (kit == null)
            {
                CloseImport();
                return;
            }

            if (importSource == null)
            {
                SetImportStatus("Choose a picture first.");
                return;
            }

            string name = KitImageStore.SanitizeName(txtImportName?.Text);
            if (name.Length == 0)
            {
                SetImportStatus("Give the image a name (letters, digits, _ and -).");
                return;
            }

            bool overwrite = File.Exists(KitImageStore.PathFor(name));
            if (overwrite && !Confirm("overwrite-image:" + name, string.Empty))
            {
                SetImportStatus($"An image called '{name}' already exists. Click again to replace it.");
                return;
            }

            try
            {
                KitImageStore.SavePortrait(importSource, cropX, cropY, cropSize, name);
            }
            catch (Exception ex)
            {
                Log.Error($"[StarterKits] Saving kit image failed: {ex}");
                SetImportStatus("Saving failed: " + ex.Message);
                return;
            }

            kit.Image = name;
            kit.ImageAtlas = KitAtlases.File;
            MarkDirty();
            CloseImport();
            RefreshDetails();
            SetStatus($"Image '{name}' saved to Kits/Images. Press Save to keep it on '{kit.Name}'.");
        }

        private void SetImportStatus(string text)
        {
            SetLabel(lblImportStatus, text);
        }
    }
}
