# SAL (Software Abstraction Layer)

## Overview

SAL is a plugin-based architecture for building modular .NET applications, originally developed in 2009. The system provides a flexible foundation for creating extensible applications with centralized configuration management and role-based feature availability.

## Key Features

- **Centralized Settings Management**
  - Configurable settings storage with default file system provider
  - Extensible through custom `ISettingsProvider` implementations
  - Role-based access control for settings

- **Dynamic Module Loading**
  - Plugin-based architecture for extensible functionality
  - Default loading from application directory
  - Customizable through `IPluginProvider` interface
  - Support for runtime module updates

- **Flexible UI Integration**
  - Multiple interface paradigms (SDI/MDI/Dialog)
  - Support for WinForms UserControls (including WPF)
  - Visual Studio IDE integration capabilities
  - Automatic UI state persistence

- **Cross-Platform Architecture**
  - Core functionality usable in WinForms, WPF, ASP.NET, Windows Services etc.
  - Shared plugin ecosystem across different application types
  - Support for system services like autorun, single instance, timers

## Architecture Overview

### Component Hierarchy

1. **Application Layer**
   - Supports multiple hosts:
      - MDI Applications
        1. [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI/)
        2. [Flatbed.MDI (Avalon)](https://dkorablin.github.io/Flatbed-MDI-Avalon/)
      - Dialog-based Applications
        1. [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
        2. [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite/)
      - Visual Studio Integration (EnvDTE)
      - Windows Services
        1. [Flatbed.Service](https://dkorablin.github.io/Flatbed-WorkerService/)
      - ASP.NET Applications

2. **SAL Host**
   - Manages plugin communication and lifecycle
   - Handles plugin loading and initialization

3. **Plugins**
   - Platform-agnostic design
   - Inter-plugin communication support
   - Host-independent operation

### Plugin System

The architecture is built around plugins implementing the `IPlugin` interface, with key characteristics:

- Modular dependency management
- Runtime plugin updates without recompilation
- No manual binding redirect configuration needed
- State persistence between sessions

### Core Components

- **SAL.Flatbed.dll**
  - Core assembly containing base interfaces
  - Minimal plugin implementation requirements

- **SAL.Windows.dll**
  - WinForms-specific components
  - Extended UI functionality

## Implementation Details

### Plugin Initialization Process

1. **Provider Loading**
   - Loads and configures settings providers (`ISettingsProvider`)
   - Initializes plugin providers (`IPluginProvider`)

2. **Kernel Plugin Detection  (optional)**
   - Identifies and initializes kernel plugins (`IPluginKernel`)
   - Establishes BLL/DAL foundations

3. **Plugin Initialization**
   - Resolves plugin dependencies
   - Establishes inter-plugin communications

### Plugin Integration Examples

#### Basic Plugin Structure
```csharp
public class MyPlugin : IPlugin
{
    private readonly IHost _host;

    public MyPlugin(IHost host)
        => this._host = host;//Optional constructor

    public Boolean OnConnection(ConnectMode mode)
        => return true;

    public Boolean OnDisconnection(DisconnectMode mode)
        => return true;
}
```

#### Settings Management

```csharp
public class Plugin : IPlugin, IPluginSettings<PluginSettings>
{
    private PluginSettings _settings;
    
    public PluginSettings Settings
    {
        get
        {
            if(_settings == null)
            {
                _settings = new PluginSettings(this);
                Host.Plugins.Settings(this).LoadAssemblyParameters(_settings);
            }
            return _settings;
        }
    }
    
    Object IPluginSettings.Settings { get { return Settings; } }
}
```

#### Data Persistence

```csharp
// Loading data
using(Stream stream = Plugin.Host.Plugins.Settings(Plugin).LoadAssemblyBlob("DataSet.xml"))
    if(stream != null)
        DataSet.ReadXml(stream);

// Saving data
using(MemoryStream stream = new MemoryStream())
{
    DataSet.WriteXml(stream);
    Plugin.Host.Plugins.Settings(this).SaveAssemblyBlob("DataSet.xml", stream);
}
```

## Platform Support

- .NET Framework 2.0
- .NET Standard 2.0

## Contributing

Contributions are welcome. Please ensure your changes maintain compatibility with the supported .NET versions.