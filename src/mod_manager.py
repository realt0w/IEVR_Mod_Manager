"""Mod management logic for scanning and organizing mods."""
import os
import json
from typing import List, Dict, Optional
from .config import DEFAULT_MODS_DIR
from .models import ModEntry


class ModManager:
    """Manages mod scanning and organization."""
    
    def __init__(self, mods_dir: str = None):
        """
        Initialize the mod manager.
        
        Args:
            mods_dir: Directory containing mods (defaults to DEFAULT_MODS_DIR)
        """
        self.mods_dir = mods_dir or DEFAULT_MODS_DIR
    
    def scan_mods(self, saved_mods: Optional[List[Dict]] = None, 
                  existing_entries: Optional[List[ModEntry]] = None) -> List[ModEntry]:
        """
        Scan the mods directory and return a list of ModEntry objects.
        
        Args:
            saved_mods: Previously saved mod configuration
            existing_entries: Existing mod entries to preserve enabled state
            
        Returns:
            List of ModEntry objects
        """
        mods_root = os.path.abspath(self.mods_dir)
        os.makedirs(mods_root, exist_ok=True)
        
        # Get all mod directories
        mod_names = [n for n in os.listdir(mods_root) 
                    if os.path.isdir(os.path.join(mods_root, n))]
        
        # Preserve enabled state from existing entries
        old_map = {me.name: me.enabled.get() for me in (existing_entries or [])}
        
        # Preserve enabled state from saved config
        saved_map = {m["name"]: m.get("enabled", True) 
                    for m in (saved_mods or [])}
        
        # Preserve order from saved config
        saved_order = [m["name"] for m in (saved_mods or [])]
        ordered_names = ([n for n in saved_order if n in mod_names] + 
                        [n for n in mod_names if n not in saved_order])
        
        # Create mod entries
        mod_entries = []
        for name in ordered_names:
            mod_path = os.path.join(mods_root, name)
            mod_data = self._load_mod_metadata(mod_path)
            
            # Determine enabled state (priority: existing > saved > default)
            if name in old_map:
                enabled = old_map[name]
            elif name in saved_map:
                enabled = saved_map[name]
            else:
                enabled = True
            
            mod_entry = ModEntry(
                name=name,
                path=mod_path,
                enabled=enabled,
                display_name=mod_data.get("display_name", name),
                author=mod_data.get("author", ""),
                mod_version=mod_data.get("mod_version", ""),
                game_version=mod_data.get("game_version", "")
            )
            mod_entries.append(mod_entry)
        
        return mod_entries
    
    def _load_mod_metadata(self, mod_path: str) -> Dict:
        """
        Load metadata from mod_data.json if it exists.
        
        Args:
            mod_path: Path to mod directory
            
        Returns:
            Dictionary with mod metadata
        """
        moddata_path = os.path.join(mod_path, "mod_data.json")
        metadata = {
            "display_name": os.path.basename(mod_path),
            "author": "",
            "mod_version": "",
            "game_version": ""
        }
        
        if not os.path.exists(moddata_path):
            return metadata
        
        try:
            with open(moddata_path, "r", encoding="utf-8") as f:
                data = json.load(f)
                metadata["display_name"] = data.get("Name") or metadata["display_name"]
                metadata["author"] = data.get("Author") or ""
                metadata["mod_version"] = data.get("ModVersion") or ""
                metadata["game_version"] = data.get("GameVersion") or ""
        except Exception:
            pass
        
        return metadata
    
    def get_enabled_mods(self, mod_entries: List[ModEntry]) -> List[str]:
        """
        Get list of paths for enabled mods.
        
        Args:
            mod_entries: List of ModEntry objects
            
        Returns:
            List of mod paths (strings)
        """
        return [me.path for me in mod_entries if me.enabled.get()]

