import { HubConnectionBuilder } from "@microsoft/signalr";
import { JSX, useEffect, useState } from "react";
import { postScannerLoggingEnabled } from "../api";

export default function Logging(): JSX.Element {
  const [loggingEnabled, setLoggingEnabled] = useState(false);
  const [messages, setMessages] = useState<string[]>([]);

  const handleCheckboxChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const isChecked = event.target.checked;
    setLoggingEnabled(isChecked);
    postScannerLoggingEnabled(isChecked);
  };

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("logging", { withCredentials: false })
      .withAutomaticReconnect()
      .build();

    connection.start()
      .then(() => console.log("Connected to SignalR"))
      .catch(err => console.error("SignalR Connection Error: ", err));

    connection.on("Logs", (message) => {
      setMessages(prev => [...prev, message]);
    });

    return () => {
      connection.stop();
    };
  }, []);

  return (
    <section
      className="d-grid"
      style={{ overflowY: 'auto', gridTemplateRows: 'auto 1fr', rowGap: '0.75rem' }}
    >
      <div className="form-check">
        <input
          id="inputLoggingEnabled"
          className="form-check-input"
          type="checkbox"
          checked={loggingEnabled}
          onChange={handleCheckboxChange}
        />
        <label className="form-check-label" htmlFor="inputLoggingEnabled">
          Logging enabled
        </label>
      </div>
      <textarea readOnly value={messages.join('\n')}></textarea>
    </section>
  );
}