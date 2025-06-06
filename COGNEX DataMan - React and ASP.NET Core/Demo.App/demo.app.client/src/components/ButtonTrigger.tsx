import { useMutation } from "@tanstack/react-query";
import { JSX } from "react";
import { useLiveDisplay } from ".";
import { postScannerTrigger } from "../api";

export default function ButtonTrigger(): JSX.Element {
  const { liveDisplay } = useLiveDisplay();

  const triggerMutation = useMutation({
    mutationFn: postScannerTrigger,
  });
  const { isPending } = triggerMutation;

  return (
    <button
      disabled={isPending || liveDisplay}
      className="btn btn-primary w-100"
      type="button"
      onMouseUp={handleOnMouseUp}
      onMouseDown={handleOnMouseDown}
    >
      Trigger
    </button>
  );

  function handleOnMouseDown(): void {
    triggerMutation.mutate(true);
  }

  function handleOnMouseUp(): void {
    triggerMutation.mutate(false);
  }
}
