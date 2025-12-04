"""Log display panel component."""
import tkinter as tk
from tkinter import ttk
from datetime import datetime


class LogPanel:
    """Panel for displaying log messages."""
    
    def __init__(self, parent):
        """
        Initialize the log panel.
        
        Args:
            parent: Parent widget
        """
        self.frame = ttk.LabelFrame(parent, text="📋 Activity Log", padding=8)
        self.frame.grid_rowconfigure(0, weight=1)
        self.frame.grid_columnconfigure(0, weight=1)
        
        # Container for text and scrollbar
        container = ttk.Frame(self.frame)
        container.grid(row=0, column=0, sticky="nsew")
        container.grid_rowconfigure(0, weight=1)
        container.grid_columnconfigure(0, weight=1)
        
        # Text widget with better styling - fixed height to prevent expansion
        self.text_widget = tk.Text(
            container, 
            height=8, 
            wrap="word", 
            font=("Consolas", 9),
            bg="#1e1e1e",
            fg="#d4d4d4",
            insertbackground="#ffffff",
            selectbackground="#264f78",
            selectforeground="#ffffff",
            borderwidth=1,
            relief="solid",
            padx=8,
            pady=8
        )
        self.text_widget.grid(row=0, column=0, sticky="ew")  # Only expand horizontally
        
        # Scrollbar
        scrollbar = ttk.Scrollbar(container, orient="vertical", command=self.text_widget.yview)
        scrollbar.grid(row=0, column=1, sticky="ns")
        self.text_widget.configure(yscrollcommand=scrollbar.set)
        
        # Configure text tags for different log levels
        self.text_widget.tag_config("success", foreground="#4ec9b0")
        self.text_widget.tag_config("error", foreground="#f48771")
        self.text_widget.tag_config("info", foreground="#569cd6")
        self.text_widget.tag_config("warning", foreground="#dcdcaa")
        self.text_widget.tag_config("timestamp", foreground="#808080", font=("Consolas", 8))
    
    def log(self, message: str, level: str = "info"):
        """
        Add a log message to the panel.
        
        Args:
            message: Message to log
            level: Log level (info, success, error, warning)
        """
        timestamp = datetime.now().strftime("%H:%M:%S")
        formatted_message = f"[{timestamp}] {message}\n"
        
        self.text_widget.insert("end", f"[{timestamp}] ", "timestamp")
        self.text_widget.insert("end", f"{message}\n", level)
        self.text_widget.see("end")
        
        # Limit log size to prevent memory issues (keep last 1000 lines)
        lines = int(self.text_widget.index("end-1c").split(".")[0])
        if lines > 1000:
            self.text_widget.delete("1.0", f"{lines - 1000}.0")
    
    def clear(self):
        """Clear all log messages."""
        self.text_widget.delete("1.0", "end")

