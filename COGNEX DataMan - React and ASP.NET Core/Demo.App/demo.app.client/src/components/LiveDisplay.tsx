import { useMutation } from "@tanstack/react-query";
import { JSX } from "react";
import { useLiveDisplay } from ".";
import { postScannerLiveDisplayEnabled } from "../api";

export default function LiveDisplay(): JSX.Element {
  const { liveDisplay, setLiveDisplay } = useLiveDisplay();

  const liveDisplayCheckboxMutation = useMutation({
    mutationFn: postScannerLiveDisplayEnabled
  });
  const { isPending } = liveDisplayCheckboxMutation;

  return (
    <div className="form-check m-0">
      <input
        id="inputLiveDisplay"
        className="form-check-input"
        type="checkbox"
        checked={liveDisplay}
        onChange={handleCheckboxChange}
        disabled={isPending}
      />
      <label className="form-check-label" htmlFor="inputLiveDisplay">
        Live Display
      </label>
    </div>
  );

  function handleCheckboxChange(event: React.ChangeEvent<HTMLInputElement>) {
    const isChecked = event.target.checked;
    setLiveDisplay(isChecked);
    liveDisplayCheckboxMutation.mutate(isChecked);
  }
}