import { useMutation } from "@tanstack/react-query";
import { JSX, useMemo } from "react";
import { useForm } from "react-hook-form";
import { postRefreshScannersList, postScannerConnect, postScannerDisconnect } from "../api";

type Inputs = {
  device: string;
  password: string;
  runKeepAliveThread: boolean;
  autoReconnect: boolean;
};

export default function FormConnect(): JSX.Element {
  const { register, handleSubmit, reset, formState: { errors } } = useForm<Inputs>();

  const disconnectMutation = useMutation({
    mutationFn: postScannerDisconnect,
    onSuccess: () => {
      reset();
    },
  });

  const refreshMutation = useMutation({
    mutationFn: postRefreshScannersList,
  });

  const connectMutation = useMutation({
    mutationFn: postScannerConnect,
    onSuccess: () => {
      reset();
    },
  });

  const isPending = useMemo(() => {
    return disconnectMutation.isPending || refreshMutation.isPending || connectMutation.isPending;
  }, [disconnectMutation.isPending, refreshMutation.isPending, connectMutation.isPending]);

  const onSubmit = (data: Inputs) => {
    connectMutation.mutate(data);
  };

  return (
    <form
      id="formConnect"
      className="d-grid"
      style={{
        overflowY: 'auto',
        gridTemplateColumns: '1fr 1fr',
        gap: '0.75rem',
      }}
      onSubmit={handleSubmit(onSubmit)}
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
                {...register("device", { required: true })}
              />
              {errors.device && <span>This field is required</span>}
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
                {...register("password", { required: true })}
              />
              {errors.password && <span>This field is required</span>}
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
                  {...register("runKeepAliveThread")}
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
                  {...register("autoReconnect")}
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
            disabled={isPending}
          >
            Connect
          </button>
        </div>
        <div className="mb-3">
          <button
            className="btn btn-secondary w-100"
            type="button"
            onClick={() => disconnectMutation.mutate()}
            disabled={isPending}
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
    </form>
  );
}