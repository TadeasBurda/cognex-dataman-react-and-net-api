import {
    ButtonTrigger,
    FormConnect,
    LiveDisplay,
    LiveDisplayProvider,
    ReceivedLogs,
    ShowImage,
    ShowReceivedString,
} from "./components";

function App() {
  return (
    <>
      <section
        className="d-grid"
        style={{
          overflowY: "auto",
          gridTemplateColumns: "1fr 1fr",
          columnGap: "0.75rem",
        }}
      >
        <section
          className="d-grid"
          style={{ overflowY: "auto", gridTemplateRows: "2fr 1fr" }}
        >
          <FormConnect />
          <textarea readOnly></textarea>
        </section>
        <section
          className="d-grid"
          style={{
            overflowY: "auto",
            gridTemplateRows: "auto 1fr",
            rowGap: "0.75rem",
          }}
        >
          <section
            className="d-grid align-items-center"
            style={{ gridTemplateColumns: "1fr 1fr", columnGap: "0.75rem" }}
          >
            <LiveDisplayProvider>
              <ButtonTrigger />
              <LiveDisplay />
            </LiveDisplayProvider>
          </section>
          <ShowImage />
          <ShowReceivedString />
        </section>
      </section>
      <ReceivedLogs />
    </>
  );
}

export default App;
