import { HubConnectionBuilder } from "@microsoft/signalr";
import { useMutation } from "@tanstack/react-query";
import { JSX, useEffect, useState } from "react";
import { postRefreshScannersList } from "../api";

interface Connector {
  name: string;
  serialNumber: string;
}

interface EthSystemConnector extends Connector {
  ipAddress: string;
  port: number;
};

interface SerSystemConnector extends Connector {
  portName: string;
  baudrate: number;
}

interface Props {
  disabled: boolean;
  onClick: (e: EthSystemConnector | SerSystemConnector) => void;
}
export default function DevicesList(props: Readonly<Props>): JSX.Element {
  const { disabled, onClick } = props;

  const [messages, setMessages] = useState<Array<EthSystemConnector | SerSystemConnector>>([]);

  const refreshMutation = useMutation({
    mutationFn: postRefreshScannersList,
  });
  const { isPending } = refreshMutation;

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("scanner", { withCredentials: false })
      .withAutomaticReconnect()
      .build();

    connection
      .start()
      .then(() => console.log("Connected to SignalR"))
      .catch((err) => console.error("SignalR Connection Error: ", err));

    connection.on("List", (message) => {
      setMessages((prev) => [...prev, JSON.parse(message)]);
    });

    return () => {
      connection.stop();
    };
  }, []);

  return (
    <fieldset className="d-flex flex-column p-1" style={{ overflowY: "auto" }}>
      <div className="flex-fill mb-3" style={{ overflowY: "auto" }}>
        <div className="list-group list-group-flush">
          {
            messages.map((message) => (
              <button
                key={message.serialNumber}
                type="button"
                className="list-group-item list-group-item-action"
                onClick={() => onClick(message)}
                disabled={disabled || isPending}
              >
                <span>{message.name}</span>
              </button>
            ))
          }
        </div>
      </div>
      <div className="mb-3">
        <button
          className="btn btn-primary w-100"
          type="button"
          onClick={() => refreshMutation.mutate()}
          disabled={disabled || isPending}
        >
          Refresh
        </button>
      </div>
    </fieldset>
  );
}
