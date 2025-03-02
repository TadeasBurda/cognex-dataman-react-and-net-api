export async function postRefreshScannersList() {
  await fetch("/api/scanners/list/refresh", {
    method: "POST",
  });
}
