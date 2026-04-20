#!/usr/bin/env bash
set -euo pipefail

# StarterKits deploy script
# Copies runtime files to deploy directory and creates a zip archive.

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Override via environment variables if needed:
# DEPLOY_DIR="/path/to/StarterKits" ZIP_PATH="/path/to/StarterKits.zip" ./deployscript.sh
# INCLUDE_PDB=1 ./deployscript.sh
DEPLOY_DIR="${DEPLOY_DIR:-/home/erano/Documents/StarterKits}"
ZIP_PATH="${ZIP_PATH:-/home/erano/Documents/StarterKits.zip}"
INCLUDE_PDB="${INCLUDE_PDB:-0}"

echo "[StarterKits] Source      : ${ROOT_DIR}"
echo "[StarterKits] Deploy dir  : ${DEPLOY_DIR}"
echo "[StarterKits] Zip path    : ${ZIP_PATH}"
echo "[StarterKits] Include PDB : ${INCLUDE_PDB}"

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
