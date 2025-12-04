"""Downloads window for displaying download links."""
import tkinter as tk
from tkinter import ttk
import webbrowser
from ..config import VIOLA_RELEASE_URL, CPK_LIST_URL


class DownloadsWindow(tk.Toplevel):
    """Window for displaying download links."""
    
    def __init__(self, parent):
        """
        Initialize the downloads window.
        
        Args:
            parent: Parent window
        """
        super().__init__(parent)
        self.title("📥 Downloads")
        self.geometry("600x300")
        self.resizable(False, False)
        
        # Center window on screen
        self.update_idletasks()
        width = self.winfo_width()
        height = self.winfo_height()
        x = (self.winfo_screenwidth() // 2) - (width // 2)
        y = (self.winfo_screenheight() // 2) - (height // 2)
        self.geometry(f"{width}x{height}+{x}+{y}")
        
        # Configure window background
        self.bg_color = "#f5f5f5"
        self.accent_color = "#0078d4"
        self.accent_hover = "#005a9e"
        self.text_color = "#323130"
        self.configure(bg=self.bg_color)
        
        # Configure styles
        self.style = ttk.Style(self)
        self.style.theme_use("clam")
        self.style.configure("TFrame", background=self.bg_color)
        self.style.configure("TLabel", background=self.bg_color, foreground=self.text_color)
        
        # Main frame
        main_frame = ttk.Frame(self, padding=20)
        main_frame.pack(fill="both", expand=True)
        main_frame.columnconfigure(0, weight=1)
        
        # Title
        title_label = tk.Label(
            main_frame,
            text="📥 Download Links",
            font=("Segoe UI", 14, "bold"),
            bg=self.bg_color,
            fg=self.text_color
        )
        title_label.grid(row=0, column=0, sticky="w", pady=(0, 20))
        
        # Links container
        links_frame = ttk.Frame(main_frame)
        links_frame.grid(row=1, column=0, sticky="ew", pady=10)
        links_frame.columnconfigure(0, weight=1)
        
        self._build_downloads_content(links_frame)
        
        # Close button
        close_btn = ttk.Button(
            main_frame,
            text="Close",
            command=self.destroy,
            width=15
        )
        close_btn.grid(row=2, column=0, pady=(20, 0))
    
    def _build_downloads_content(self, parent):
        """Build the downloads links content."""
        link_font = ("Segoe UI", 10)
        link_fg = self.accent_color
        link_hover = self.accent_hover
        
        def make_link(label, url, row_idx):
            container = tk.Frame(parent, bg=self.bg_color, highlightthickness=0)
            container.grid(row=row_idx, column=0, sticky="w", pady=8)
            
            icon = "🔗"
            l = tk.Label(
                container,
                text=f"{icon} {label}",
                fg=link_fg,
                cursor="hand2",
                font=link_font,
                bg=self.bg_color,
                anchor="w",
                highlightthickness=0
            )
            l.pack(side="left")
            
            def on_enter(e):
                l.config(fg=link_hover, font=("Segoe UI", 10, "underline"))
            
            def on_leave(e):
                l.config(fg=link_fg, font=link_font)
            
            l.bind("<Button-1>", lambda e: webbrowser.open_new(url))
            l.bind("<Enter>", on_enter)
            l.bind("<Leave>", on_leave)
        
        make_link("Download Viola.CLI-Portable.exe", VIOLA_RELEASE_URL, 0)
        make_link("Download cpk_list.cfg.bin", CPK_LIST_URL, 1)

