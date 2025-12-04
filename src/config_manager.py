"""Configuration management for loading and saving settings."""
import os
import json
from typing import Dict, List, Optional
from .config import CONFIG_PATH, DEFAULT_TMP_DIR
from .models import ModEntry


class ConfigManager:
    """Manages application configuration loading and saving."""
    
    def __init__(self):
        """Initialize the config manager."""
        self.config_path = CONFIG_PATH
    
    def load(self) -> Dict:
        """
        Load configuration from file.
        
        Returns:
            Dictionary containing configuration values
        """
        if not os.path.exists(self.config_path):
            return self._get_default_config()
        
        try:
            with open(self.config_path, "r", encoding="utf-8") as f:
                config = json.load(f)
            return config
        except Exception as e:
            print(f"Error loading config: {e}")
            return self._get_default_config()
    
    def save(self, game_path: str, cfgbin_path: str, violacli_path: str, 
             tmp_dir: str, mod_entries: List[ModEntry]) -> bool:
        """
        Save configuration to file.
        
        Args:
            game_path: Path to game directory
            cfgbin_path: Path to cpk_list.cfg.bin
            violacli_path: Path to Viola.CLI-Portable.exe
            tmp_dir: Temporary directory path
            mod_entries: List of mod entries
            
        Returns:
            True if saved successfully, False otherwise
        """
        config = {
            "game_path": game_path,
            "cfgbin_path": cfgbin_path,
            "violacli_path": violacli_path,
            "tmp_dir": tmp_dir,
            "mods": [me.to_dict() for me in mod_entries]
        }
        
        try:
            with open(self.config_path, "w", encoding="utf-8") as f:
                json.dump(config, f, ensure_ascii=False, indent=2)
            return True
        except Exception as e:
            print(f"Error saving config: {e}")
            return False
    
    def _get_default_config(self) -> Dict:
        """Get default configuration values."""
        return {
            "game_path": "",
            "cfgbin_path": "",
            "violacli_path": "",
            "tmp_dir": DEFAULT_TMP_DIR,
            "mods": []
        }

