// TODO: After disconnect, reset logs and everything else

import { useMutation } from "@tanstack/react-query";
import { JSX, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { EthSystemConnector, ScannersList, SerSystemConnector } from ".";
import {
  postScannerConnectEth,
  postScannerConnectSer,
  postScannerDisconnect,
} from "../api";

type Inputs = {
  device: string;
  password: string;
  runKeepAliveThread: boolean;
  autoReconnect: boolean;
};

export default function FormConnect(): JSX.Element {
  const [scanner, setScanner] = useState<
    EthSystemConnector | SerSystemConnector | null
  >(null);

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    formState: { errors },
  } = useForm<Inputs>({
    defaultValues: {
      device: "",
      password: "",
      runKeepAliveThread: false,
      autoReconnect: false,
    },
  });

  const disconnectMutation = useMutation({
    mutationFn: postScannerDisconnect,
    onSuccess: () => {
      reset();
    },
  });

  const connectEthMutation = useMutation({
    mutationFn: postScannerConnectEth,
    onSuccess: () => {
      reset();
    },
  });

  const connectSerMutation = useMutation({
    mutationFn: postScannerConnectSer,
    onSuccess: () => {
      reset();
    },
  });

  const isPending = useMemo(() => {
    return (
      disconnectMutation.isPending ||
      connectEthMutation.isPending ||
      connectSerMutation.isPending
    );
  }, [
    disconnectMutation.isPending,
    connectEthMutation.isPending,
    connectSerMutation.isPending,
  ]);

  return (
    <form
      id="formConnect"
      className="d-grid"
      style={{
        overflowY: "auto",
        gridTemplateColumns: "1fr 1fr",
        gap: "0.75rem",
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
                {...register("device", { required: true, disabled: true })}
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
                {...register("password")}
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
      <ScannersList onClick={handleOnClickScanner} disabled={isPending} />
    </form>
  );

  function onSubmit(data: Inputs): void {
    if (scanner) {
      if (isEthSystemConnector(scanner)) {
        connectEthMutation.mutate({
          ipAddress: scanner.ipAddress,
          port: scanner.port,
          password: data.password,
          runKeepAliveThread: data.runKeepAliveThread,
          autoReconnect: data.autoReconnect,
        });
      } else {
        connectSerMutation.mutate({
          portName: scanner.portName,
          baudrate: scanner.baudrate,
          password: data.password,
          runKeepAliveThread: data.runKeepAliveThread,
          autoReconnect: data.autoReconnect,
        });
      }
    }
  }

  function isEthSystemConnector(
    connector: EthSystemConnector | SerSystemConnector,
  ): connector is EthSystemConnector {
    return (connector as EthSystemConnector).ipAddress !== undefined;
  }

  function handleOnClickScanner(
    e: EthSystemConnector | SerSystemConnector,
  ): void {
    setValue("device", isEthSystemConnector(e) ? e.ipAddress : e.portName);
    setScanner(e);
  }
}
