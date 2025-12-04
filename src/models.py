"""Data models for the mod manager."""
import tkinter as tk


class ModEntry:
    """Represents a mod entry in the manager."""
    
    def __init__(self, name, path, enabled=True, display_name=None, 
                 author=None, mod_version=None, game_version=None):
        """
        Initialize a mod entry.
        
        Args:
            name: Internal mod name (folder name)
            path: Full path to mod directory
            enabled: Whether mod is enabled
            display_name: Display name (defaults to name)
            author: Mod author
            mod_version: Mod version string
            game_version: Game version string
        """
        self.name = name
        self.path = path
        self.enabled = tk.BooleanVar(value=enabled)
        self.display_name = display_name or name
        self.author = author or ""
        self.mod_version = mod_version or ""
        self.game_version = game_version or ""
    
    def to_dict(self):
        """Convert mod entry to dictionary for serialization."""
        return {
            "name": self.name,
            "enabled": self.enabled.get()
        }
    
    @classmethod
    def from_dict(cls, data, path):
        """Create ModEntry from dictionary."""
        return cls(
            name=data["name"],
            path=path,
            enabled=data.get("enabled", True)
        )

