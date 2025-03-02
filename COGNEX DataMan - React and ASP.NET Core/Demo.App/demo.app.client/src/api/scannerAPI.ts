export async function postScannerTrigger(on: boolean): Promise<void> {
  await fetch(`/api/scanner/trigger?on=${on}`, {
    method: "POST",
  });
}

export async function postScannerLoggingEnabled(
  enable: boolean,
): Promise<void> {
  await fetch(`/api/scanner/logging?enable=${enable}`, {
    method: "POST",
  });
}

export async function postScannerLiveDisplayEnabled(
  enable: boolean,
): Promise<void> {
  await fetch(`/api/scanner/live-display?enable=${enable}`, {
    method: "POST",
  });
}

interface PostScannnerConnectEthBody {
  ipAddress: string;
  port: number;
  password: string;
  runKeepAliveThread: boolean;
  autoReconnect: boolean;
}
export async function postScannerConnectEth(
  body: Readonly<PostScannnerConnectEthBody>,
): Promise<void> {
  await fetch("/api/scanner/connect/eth", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });
}

interface PostScannnerConnectSerBody {
  portName: string;
  baudrate: number;
  password: string;
  runKeepAliveThread: boolean;
  autoReconnect: boolean;
}
export async function postScannerConnectSer(
  body: Readonly<PostScannnerConnectSerBody>,
): Promise<void> {
  await fetch("/api/scanner/connect/ser", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });
}

export async function postScannerDisconnect() {
  await fetch("/api/scanner/disconnect", {
    method: "POST",
  });
}
