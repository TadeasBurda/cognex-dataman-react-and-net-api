import { FormConnect, Logging, ShowImage } from "./components";

function App() {
  return (
    <>
      <section
        className="d-grid"
        style={{
          overflowY: 'auto',
          gridTemplateColumns: '1fr 1fr',
          columnGap: '0.75rem',
        }}
      >
        <section
          className="d-grid"
          style={{ overflowY: 'auto', gridTemplateRows: '2fr 1fr' }}
        >
          <FormConnect />
          <textarea readOnly></textarea>
        </section>
        <section
          className="d-grid"
          style={{ overflowY: 'auto', gridTemplateRows: 'auto 1fr', rowGap: '0.75rem' }}
        >
          <section
            className="d-grid align-items-center"
            style={{ gridTemplateColumns: '1fr 1fr', columnGap: '0.75rem' }}
          >
            <button className="btn btn-primary w-100" type="button">
              Trigger
            </button>
            <div className="form-check m-0">
              <input
                id="formCheck-2"
                className="form-check-input"
                type="checkbox"
              />
              <label className="form-check-label" htmlFor="formCheck-2">
                Live Display
              </label>
            </div>
          </section>
          <ShowImage />
          <p>lbReadString</p>
        </section>
      </section>
      <Logging  />
    </>
  );
}

export default App;