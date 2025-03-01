export async function postLoggingEnabled(enable: boolean): Promise<void> {
  await fetch(`/api/logging?enable=${enable}`, {
    method: 'POST'
  });
}