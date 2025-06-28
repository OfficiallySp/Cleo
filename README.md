# Cleo - Your AI Assistant

Cleo is a fast, efficient Windows desktop AI assistant that lives in your system tray. Inspired by Spotlight search, it provides quick access to AI-powered responses with beautiful animations and a modern UI.

## Features

- 🚀 **Lightning Fast**: Instant access via `Ctrl+Space` hotkey
- 🎤 **Voice Input**: Speak your questions naturally
- 💬 **Text Input**: Type queries with a modern, responsive interface
- 🎨 **Beautiful Animations**: Smooth transitions and expressive UI elements
- 🔌 **Local AI Model**: Uses smollm2:135m-instruct-q3_K_S for privacy and speed
- 📌 **System Tray Integration**: Always available, never intrusive

## Prerequisites

1. **Windows 10/11** (64-bit)
2. **.NET 8.0 Runtime** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
3. **Ollama** - Local LLM runtime
   ```powershell
   # Install Ollama from https://ollama.ai
   # Then pull the model:
   ollama pull smollm2:135m-instruct-q3_K_S
   ```

## Quick Start

1. **Clone the repository**
   ```powershell
   git clone https://github.com/yourusername/cleo.git
   cd cleo
   ```

2. **Build the application**
   ```powershell
   dotnet build
   ```

3. **Run Cleo**
   ```powershell
   dotnet run
   ```

4. **Usage**
   - Press `Ctrl+Space` to open Cleo
   - Type or speak your question
   - Press `Enter` to submit
   - Press `ESC` to hide

## Building for Production

Create a self-contained executable:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be in `bin\Release\net8.0-windows\win-x64\publish\`

## Configuration

### AI Model Settings
The AI service is configured to use Ollama's default port (11434). To change this, edit `Services/AIService.cs`:
```csharp
_modelEndpoint = "http://localhost:11434/api/generate";
```

### Hotkey Customization
To change the global hotkey, edit `App.xaml.cs`:
```csharp
_hotKeyManager.RegisterHotKey(Helpers.ModifierKeys.Control, () => ...);
```

## Architecture

- **WPF** for the UI layer with hardware-accelerated animations
- **Windows Forms** for system tray integration
- **HTTP Client** for AI model communication
- **Speech Recognition** using System.Speech

## Troubleshooting

### "Ollama not found" error
Ensure Ollama is running:
```powershell
ollama serve
```

### Voice input not working
Check Windows microphone permissions:
1. Go to Settings → Privacy → Microphone
2. Enable microphone access for desktop apps

### High DPI display issues
The app is DPI-aware but if you experience scaling issues, right-click the .exe → Properties → Compatibility → Change high DPI settings.

## Contributing

Pull requests are welcome! Please ensure:
- Code follows C# conventions
- UI changes maintain the modern, minimal aesthetic
- Performance optimizations are measurable

## License

MIT License - see LICENSE file for details
