"""Configuration constants and paths."""
import os
import sys

# Application configuration
APP_CONFIG = "config.json"
MODS_DIRNAME = "Mods"
TMP_DIRNAME = "tmp"

# Determine base directory
if getattr(sys, "frozen", False):
    BASE_DIR = os.path.dirname(sys.executable)
else:
    BASE_DIR = os.path.dirname(os.path.dirname(__file__))

# Paths
CONFIG_PATH = os.path.join(BASE_DIR, APP_CONFIG)
DEFAULT_MODS_DIR = os.path.join(BASE_DIR, MODS_DIRNAME)
DEFAULT_TMP_DIR = os.path.join(BASE_DIR, TMP_DIRNAME)

# UI Configuration
WINDOW_TITLE = "IEVR Mod Manager"
WINDOW_SIZE = "1200x800"
WINDOW_MIN_SIZE = (1000, 600)

# Links
VIOLA_RELEASE_URL = "https://github.com/skythebro/Viola/releases/latest"
CPK_LIST_URL = "https://github.com/Adr1GR/IEVR_Mod_Manager/tree/main/cpk_list"

