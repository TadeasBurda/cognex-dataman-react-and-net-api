import { HubConnectionBuilder } from "@microsoft/signalr";
import { JSX, useEffect, useState } from "react";

export default function ShowReceivedString(): JSX.Element {
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("scanner", { withCredentials: false })
      .withAutomaticReconnect()
      .build();

    connection
      .start()
      .then(() => console.log("Connected to SignalR"))
      .catch((err) => console.error("SignalR Connection Error: ", err));

    connection.on("Received", (message) => {
      setMessage(message);
    });

    return () => {
      connection.stop();
    };
  }, []);

  return <p>{message}</p>;
}