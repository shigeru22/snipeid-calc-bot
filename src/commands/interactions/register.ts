import dotenv from "dotenv";
import { initializeInteractionCommands } from ".";
import { Environment } from "../../utils";
import { Log } from "../../utils/log";

dotenv.config();

(async () => {
  if(!Environment.validateEnvironmentVariables()) {
    process.exit(1);
  }

  Log.info("main", "Initializing bot interaction commands...");

  try {
    await initializeInteractionCommands();
  }
  catch (e) {
    if(e instanceof Error) {
      Log.error("main", `An error occurred while registering commands. Error details below.\n${ e.stack }`);
    }
    else {
      Log.error("main", "An unknown error occurred while registering commands.");
    }

    process.exit(1);
  }

  Log.info("main", "Command registration completed.");
})();
