"""Run the engine RSI checks with the additional licenses used by Dumont assets."""

import json
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path


def main():
    schemas = Path(__file__).resolve().parents[1] / "RobustToolbox" / "Schemas"
    schema = json.loads((schemas / "rsi.json").read_text(encoding="utf-8"))
    schema["properties"]["license"]["enum"].append("AGPL-3.0-or-later")
    with tempfile.TemporaryDirectory(prefix="dumont-rsi-") as temporary:
        directory = Path(temporary)
        validator = directory / "validate_rsis.py"
        shutil.copyfile(schemas / "validate_rsis.py", validator)
        (directory / "rsi.json").write_text(json.dumps(schema), encoding="utf-8")
        return subprocess.call([sys.executable, str(validator), *sys.argv[1:]])


if __name__ == "__main__":
    sys.exit(main())
