import { HubConnectionBuilder } from "@microsoft/signalr";
import { JSX, useEffect, useState } from "react";

export default function ConnectLogs(): JSX.Element {
  const [messages, setMessages] = useState<string[]>([]);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("logging", { withCredentials: false })
      .withAutomaticReconnect()
      .build();

    connection.start()
      .then(() => console.log("Connected to SignalR"))
      .catch(err => console.error("SignalR Connection Error: ", err));

    connection.on("ConnectLogs", (message) => {
      setMessages(prev => [...prev, message]);
    });

    return () => {
      connection.stop();
    };
  }, []);

  return (<textarea readOnly value={messages.join('\n')}></textarea>);
}