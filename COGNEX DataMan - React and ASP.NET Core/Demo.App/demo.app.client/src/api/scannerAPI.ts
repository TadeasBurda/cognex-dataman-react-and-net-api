export async function postScannerLoggingEnabled(
  enable: boolean,
): Promise<void> {
  await fetch(`/api/scanner/logging?enable=${enable}`, {
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
  props: Readonly<PostScannnerConnectEthBody>,
): Promise<void> {
  const fromBody = new FormData();
  fromBody.append("ipAddress", props.ipAddress);
  fromBody.append("port", props.port.toString());
  fromBody.append("password", props.password);
  fromBody.append("runKeepAliveThread", props.runKeepAliveThread.toString());
  fromBody.append("autoReconnect", props.autoReconnect.toString());

  await fetch("/api/scanner/connect/eth", {
    method: "POST",
    body: fromBody,
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
  props: Readonly<PostScannnerConnectSerBody>,
): Promise<void> {
  const fromBody = new FormData();
  fromBody.append("portName", props.portName);
  fromBody.append("baudrate", props.baudrate.toString());
  fromBody.append("password", props.password);
  fromBody.append("runKeepAliveThread", props.runKeepAliveThread.toString());
  fromBody.append("autoReconnect", props.autoReconnect.toString());

  await fetch("/api/scanner/connect/eth", {
    method: "POST",
    body: fromBody,
  });
}

export async function postScannerDisconnect() {
  await fetch("/api/scanner/disconnect", {
    method: "POST",
  });
}
