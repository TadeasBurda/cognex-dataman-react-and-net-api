import { JSX } from "react";

export default function DevicesList(): JSX.Element {
  return (
    <fieldset className="d-flex flex-column p-1" style={{ overflowY: "auto" }}>
      <div className="flex-fill mb-3" style={{ overflowY: "auto" }}>
        <div className="list-group list-group-flush">
          <button
            type="button"
            className="list-group-item list-group-item-action"
            onClick={() => refreshMutation.mutate()}
            disabled={isPending}
          >
            <span>List Group Item</span>
          </button>
        </div>
      </div>
      <div className="mb-3">
        <button
          className="btn btn-primary w-100"
          type="button"
          onClick={() => refreshMutation.mutate()}
          disabled={isPending}
        >
          Refresh
        </button>
      </div>
    </fieldset>
  );
}
