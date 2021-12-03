import React, { memo } from "react";
import ReactDOM from "react-dom";
import Environments from "./Environments";

const EnvApp = memo((props) => {
  return <Environments />;
});

const rootElement = document.getElementById("root");
ReactDOM.render(<EnvApp />, rootElement);

module.hot.accept();
