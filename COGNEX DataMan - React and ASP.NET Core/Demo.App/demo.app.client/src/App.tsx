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
          <form
            id="formConnect"
            className="d-grid"
            style={{
              overflowY: 'auto',
              gridTemplateColumns: '1fr 1fr',
              gap: '0.75rem',
            }}
          >
            <fieldset className="d-flex flex-column p-1">
              <div className="mb-3">
                <div className="row align-items-center">
                  <div className="col-2">
                    <label className="col-form-label" htmlFor="inputDevice">
                      Device:
                    </label>
                  </div>
                  <div className="col-10">
                    <input
                      id="inputDevice"
                      className="form-control form-control-sm"
                      type="text"
                    />
                  </div>
                </div>
              </div>
              <div className="mb-3">
                <div className="row align-items-center">
                  <div className="col-2">
                    <label className="col-form-label" htmlFor="inputPassword">
                      Password:
                    </label>
                  </div>
                  <div className="col-10">
                    <input
                      id="inputPassword"
                      className="form-control form-control-sm"
                      type="password"
                    />
                  </div>
                </div>
              </div>
              <div className="mb-3">
                <div className="row justify-content-end">
                  <div className="col-10">
                    <div className="form-check">
                      <input
                        id="inputRunKeepAliveThread"
                        className="form-check-input"
                        type="checkbox"
                      />
                      <label
                        className="form-check-label"
                        htmlFor="inputRunKeepAliveThread"
                      >
                        Run Keep Alive Thread
                      </label>
                    </div>
                  </div>
                </div>
              </div>
              <div className="mb-3">
                <div className="row justify-content-end">
                  <div className="col-10">
                    <div className="form-check">
                      <input
                        id="inputAutoReconnect"
                        className="form-check-input"
                        type="checkbox"
                      />
                      <label
                        className="form-check-label"
                        htmlFor="inputAutoReconnect"
                      >
                        Auto-reconnect
                      </label>
                    </div>
                  </div>
                </div>
              </div>
              <div className="mt-auto mb-3">
                <button
                  className="btn btn-primary w-100"
                  type="submit"
                  form="formConnect"
                >
                  Connect
                </button>
              </div>
              <div className="mb-3">
                <button
                  className="btn btn-secondary w-100"
                  type="reset"
                  form="formConnect"
                >
                  Disconnect
                </button>
              </div>
            </fieldset>
            <fieldset
              className="d-flex flex-column p-1"
              style={{ overflowY: 'auto' }}
            >
              <div className="flex-fill mb-3" style={{ overflowY: 'auto' }}>
                <div className="list-group list-group-flush">
                  <button type="button" className="list-group-item list-group-item-action">
                    <span>List Group Item</span>
                  </button>
                </div>
              </div>
              <div className="mb-3">
                <button className="btn btn-primary w-100" type="button">
                  Refresh
                </button>
              </div>
            </fieldset>
          </form>
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
          <div style={{ backgroundColor: 'var(--bs-secondary)' }}></div>
        </section>
      </section>
      <section
        className="d-grid"
        style={{ overflowY: 'auto', gridTemplateRows: 'auto 1fr', rowGap: '0.75rem' }}
      >
        <div className="form-check">
          <input
            id="formCheck-1"
            className="form-check-input"
            type="checkbox"
          />
          <label className="form-check-label" htmlFor="formCheck-1">
            Logging enabled
          </label>
        </div>
        <textarea readOnly></textarea>
      </section>
    </>
  );
}

export default App;