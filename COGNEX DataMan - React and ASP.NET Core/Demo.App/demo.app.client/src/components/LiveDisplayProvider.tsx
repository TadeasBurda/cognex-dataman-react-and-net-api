import { createContext, useContext, useState, ReactNode, useMemo } from "react";

interface LiveDisplayContextProps {
  liveDisplay: boolean;
  setLiveDisplay: (value: boolean) => void;
}

const LiveDisplayContext = createContext<LiveDisplayContextProps | undefined>(undefined);

export const LiveDisplayProvider = ({ children }: { children: ReactNode }) => {
  const [liveDisplay, setLiveDisplay] = useState<boolean>(false);

  const memoizedValue = useMemo(() => ({ liveDisplay, setLiveDisplay }), [liveDisplay, setLiveDisplay]);

  return (
    <LiveDisplayContext.Provider value={memoizedValue}>
      {children}
    </LiveDisplayContext.Provider>
  );
};

export const useLiveDisplay = () => {
  const context = useContext(LiveDisplayContext);
  if (!context) {
    throw new Error("useLiveDisplay must be used within a LiveDisplayProvider");
  }
  return context;
};
