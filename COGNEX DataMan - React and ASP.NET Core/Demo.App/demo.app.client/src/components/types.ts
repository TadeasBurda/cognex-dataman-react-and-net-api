interface Connector {
  name: string;
  serialNumber: string;
}

export interface EthSystemConnector extends Connector {
  ipAddress: string;
  port: number;
};

export interface SerSystemConnector extends Connector {
  portName: string;
  baudrate: number;
}