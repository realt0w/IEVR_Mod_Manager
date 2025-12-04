"""Integration with Viola CLI for mod merging."""
import os
import shutil
import subprocess
from typing import Callable, List, Optional


def shlex_quote(s):
    """Quote a string for shell usage."""
    try:
        import shlex
        return shlex.quote(str(s))
    except Exception:
        return str(s)


class ViolaIntegration:
    """Handles Viola CLI operations for mod merging."""
    
    def __init__(self, log_callback: Optional[Callable[[str], None]] = None):
        """
        Initialize Viola integration.
        
        Args:
            log_callback: Optional callback function for logging messages
        """
        self.log_callback = log_callback or (lambda x: None)
        self._running = False
        self._proc = None
    
    def merge_mods(self, violacli_path: str, cfgbin_path: str, 
                   mod_paths: List[str], output_dir: str) -> bool:
        """
        Merge mods using Viola CLI.
        
        Args:
            violacli_path: Path to Viola.CLI-Portable.exe
            cfgbin_path: Path to cpk_list.cfg.bin
            mod_paths: List of mod directory paths to merge
            output_dir: Output directory for merged files
            
        Returns:
            True if merge succeeded, False otherwise
        """
        if not os.path.exists(violacli_path):
            self._log("Error: violacli.exe not found")
            return False
        
        if not os.path.exists(cfgbin_path):
            self._log("Error: cpk_list.cfg.bin not found")
            return False
        
        output_dir = os.path.abspath(output_dir)
        os.makedirs(output_dir, exist_ok=True)
        
        cmd = [violacli_path, "-m", "merge", "-p", "PC", "--cl", cfgbin_path] + mod_paths + ["-o", output_dir]
        
        self._log(f"Executing command:\n{' '.join(shlex_quote(x) for x in cmd)}")
        
        try:
            CREATE_NO_WINDOW = getattr(subprocess, "CREATE_NO_WINDOW", 0)
            startupinfo = None
            
            if os.name == "nt":
                try:
                    si = subprocess.STARTUPINFO()
                    si.dwFlags |= subprocess.STARTF_USESHOWWINDOW
                    startupinfo = si
                except Exception:
                    startupinfo = None
            
            self._running = True
            proc = subprocess.Popen(
                cmd,
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
                text=True,
                creationflags=CREATE_NO_WINDOW if os.name == "nt" else 0,
                startupinfo=startupinfo
            )
            
            self._proc = proc
            
            for line in proc.stdout:
                self._log(line.rstrip())
            
            proc.wait()
            rc = proc.returncode
            self._log(f"violacli finished with code {rc}")
            
            self._proc = None
            self._running = False
            
            return rc == 0
            
        except FileNotFoundError as e:
            self._log(f"Execution error: {e}")
            self._running = False
            return False
        except Exception as e:
            self._log(f"Unexpected error: {e}")
            self._running = False
            return False
    
    def copy_merged_files(self, tmp_data_dir: str, game_data_dir: str) -> bool:
        """
        Copy merged files from temporary directory to game directory.
        
        Args:
            tmp_data_dir: Temporary data directory with merged files
            game_data_dir: Game's data directory
            
        Returns:
            True if copy succeeded, False otherwise
        """
        if not os.path.exists(tmp_data_dir) or not os.path.isdir(tmp_data_dir):
            self._log(f"{tmp_data_dir} was not found. Aborting.")
            return False
        
        self._log(f"Copying {tmp_data_dir} -> {game_data_dir} (overwriting if needed)...")
        
        os.makedirs(game_data_dir, exist_ok=True)
        
        try:
            shutil.copytree(tmp_data_dir, game_data_dir, dirs_exist_ok=True)
        except TypeError:
            # Fallback for older Python versions
            for root, dirs, files in os.walk(tmp_data_dir):
                rel = os.path.relpath(root, tmp_data_dir)
                target_dir = os.path.join(game_data_dir, rel) if rel != "." else game_data_dir
                os.makedirs(target_dir, exist_ok=True)
                for f in files:
                    srcf = os.path.join(root, f)
                    dstf = os.path.join(target_dir, f)
                    try:
                        shutil.copy2(srcf, dstf)
                    except Exception as e:
                        self._log(f"Error copying {srcf} -> {dstf}: {e}")
        
        self._log("Copy completed.")
        return True
    
    def cleanup_temp(self, tmp_data_dir: str) -> bool:
        """
        Clean up temporary directory.
        
        Args:
            tmp_data_dir: Temporary data directory to remove
            
        Returns:
            True if cleanup succeeded, False otherwise
        """
        try:
            shutil.rmtree(tmp_data_dir)
            self._log(f"Removed temporary folder {tmp_data_dir}.")
            return True
        except Exception as e:
            self._log(f"Could not remove {tmp_data_dir}: {e}")
            return False
    
    def is_running(self) -> bool:
        """Check if a merge operation is currently running."""
        return self._running
    
    def stop(self):
        """Stop the current operation if running."""
        if self._proc:
            try:
                self._proc.terminate()
            except Exception:
                pass
            self._proc = None
        self._running = False
    
    def _log(self, message: str):
        """Log a message using the callback."""
        self.log_callback(message)

