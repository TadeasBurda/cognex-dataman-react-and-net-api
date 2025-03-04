import { HubConnectionBuilder } from "@microsoft/signalr";
import { JSX, useEffect, useState } from "react";

export default function ShowImage(): JSX.Element {
  const [imageSrc, setImageSrc] = useState<string>("");

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("image", { withCredentials: false })
      .withAutomaticReconnect()
      .build();

    connection
      .start()
      .then(() => console.log("Connected to SignalR"))
      .catch((err) => console.error("SignalR Connection Error: ", err));

    connection.on("ReceiveImage", (base64Image) => {
      setImageSrc(`data:image/png;base64,${base64Image}`);
    });

    return () => {
      connection.stop();
    };
  }, []);

  return <img src={imageSrc} />;
}
