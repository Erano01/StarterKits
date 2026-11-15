#!/usr/bin/env bash
set -euo pipefail

# StarterKits deploy script
# Copies runtime files to deploy directory and creates a zip archive.

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Override via environment variables if needed:
# DEPLOY_DIR="/path/to/StarterKits" ZIP_PATH="/path/to/StarterKits.zip" ./deployscript.sh
# INCLUDE_PDB=1 ./deployscript.sh
# KITS_SOURCE=defaults ./deployscript.sh   (ship Kits/defaults.xml as-is instead of your local kits.xml)
DEPLOY_DIR="${DEPLOY_DIR:-/home/erano/Documents/StarterKits}"
ZIP_PATH="${ZIP_PATH:-/home/erano/Documents/StarterKits.zip}"
INCLUDE_PDB="${INCLUDE_PDB:-0}"
KITS_SOURCE="${KITS_SOURCE:-local}"

echo "[StarterKits] Source      : ${ROOT_DIR}"
echo "[StarterKits] Deploy dir  : ${DEPLOY_DIR}"
echo "[StarterKits] Zip path    : ${ZIP_PATH}"
echo "[StarterKits] Include PDB : ${INCLUDE_PDB}"
echo "[StarterKits] Kits source : ${KITS_SOURCE}"

# Your local Kits/kits.xml (edited with the in-game Kit Editor) becomes the shipped defaults.
# It is promoted into Kits/defaults.xml in the repo, so the release kits show up in git.
if [[ "${KITS_SOURCE}" == "local" ]]; then
	if [[ ! -f "${ROOT_DIR}/Kits/kits.xml" ]]; then
		echo "[StarterKits] ERROR: Kits/kits.xml not found. Run with KITS_SOURCE=defaults to ship defaults.xml." >&2
		exit 1
	fi

	python3 -I - "${ROOT_DIR}/Kits/kits.xml" "${ROOT_DIR}/Kits/defaults.xml" <<'PY'
import sys
import xml.etree.ElementTree as ET

src, dst = sys.argv[1], sys.argv[2]
root = ET.parse(src).getroot()  # fails loudly on broken XML
if root.tag != "starterkits" or not root.findall("kit"):
    sys.exit(f"[StarterKits] ERROR: {src} has no <kit> entries.")

with open(src, encoding="utf-8-sig") as f:
    lines = f.read().splitlines()

# Swap the "user kits" comment for the defaults header.
header = ("<!-- StarterKits default kit definitions. Shipped with the mod; do not edit.\n"
          "     Your edits are saved to Kits/kits.xml (created on first run from this file). -->")
body = [l for l in lines if not l.lstrip().startswith("<?xml") and "StarterKits user kits." not in l]
with open(dst, "w", encoding="utf-8", newline="\n") as f:
    f.write('<?xml version="1.0" encoding="UTF-8"?>\n' + header + "\n" + "\n".join(body) + "\n")
print(f"[StarterKits] Promoted {len(root.findall('kit'))} kit(s) from kits.xml to defaults.xml.")
PY
elif [[ "${KITS_SOURCE}" != "defaults" ]]; then
	echo "[StarterKits] ERROR: KITS_SOURCE must be 'local' or 'defaults'." >&2
	exit 1
fi

rm -rf "${DEPLOY_DIR}"
mkdir -p "${DEPLOY_DIR}"
mkdir -p "${DEPLOY_DIR}/Config" "${DEPLOY_DIR}/UIAtlases"

# Copy only deploy-required files/folders.
cp -a "${ROOT_DIR}/ModInfo.xml" "${DEPLOY_DIR}/"
cp -a "${ROOT_DIR}/StarterKits.dll" "${DEPLOY_DIR}/"

if [[ "${INCLUDE_PDB}" == "1" ]]; then
	cp -a "${ROOT_DIR}/StarterKits.pdb" "${DEPLOY_DIR}/"
fi

cp -a "${ROOT_DIR}/Config/." "${DEPLOY_DIR}/Config/"
cp -a "${ROOT_DIR}/UIAtlases/." "${DEPLOY_DIR}/UIAtlases/"

# Ship only the default kits. Kits/kits.xml is the player's own file (written by the
# main menu Kit Editor) and must never be overwritten by an update.
mkdir -p "${DEPLOY_DIR}/Kits"
cp -a "${ROOT_DIR}/Kits/defaults.xml" "${DEPLOY_DIR}/Kits/"

# Kits with an imported portrait (image_atlas="File") need their PNG from Kits/Images.
for image in $(grep -o '<kit [^>]*image_atlas="File"[^>]*>' "${ROOT_DIR}/Kits/defaults.xml" | sed -n 's/.* image="\([^"]*\)".*/\1/p' | sort -u); do
	png="${ROOT_DIR}/Kits/Images/${image}.png"
	if [[ ! -f "${png}" ]]; then
		echo "[StarterKits] ERROR: kit image '${image}' missing: ${png}" >&2
		exit 1
	fi
	mkdir -p "${DEPLOY_DIR}/Kits/Images"
	cp -a "${png}" "${DEPLOY_DIR}/Kits/Images/"
done

# Recreate archive from deploy folder.
rm -f "${ZIP_PATH}"
(
	cd "$(dirname "${DEPLOY_DIR}")"
	zip -r "${ZIP_PATH}" "$(basename "${DEPLOY_DIR}")" >/dev/null
)

echo "[StarterKits] Deploy completed."
echo "[StarterKits] Deploy folder contents:"
find "${DEPLOY_DIR}" -maxdepth 3 -mindepth 1 | sort
echo "[StarterKits] Zip file:"
ls -lh "${ZIP_PATH}"
