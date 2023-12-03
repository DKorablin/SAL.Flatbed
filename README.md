#  History of appearance

Development of architecture started in 2009. Few times was made full refactoring basic API layer, was added and removed key features from the system. Since the beginning of 2012, system is basically not changed. From which is follows that the base has become quite stable.

While creating backoffice software for the few companies I formulated the minimal features that has to be supported by any backoffice applications:

1.  *Centralized storage settings*\
    For which user settings stored on the different server and with fixed users roles. User can read this settings and with specific roles they can be changed.
2.  *Show UI with specified feature that available for the user according to his role.*\
    I'm assuming 2 use cases for this approach:
    1.  Application contains all logic and shows the desired menu items based on the read settings.\
        With the user knowledge about internal structure of the application, user can not only see all interface, witch he can't see, but also read remote calls. With this knowledge user can invoke functions that he can't see without appropriate security measures.
    2.  Application compiled separately for each roles. With result that application will contains only features required for current user. In this approach it will be hard to manage user roles. Because user role changes will require to reinstall application.

After analyzing these requirements, I came up with the idea to write a unified interface that solves a minimum number of tasks:

1.  Centralized settings storage (if required).\
    By default file system is used to store user storage. But if required, we can use or write our own plugin based on interface from basic layer ISettingsProvider that will allow to load and save settings from other data source.
2.  Download required modules for UI and internal logic process by user or server role.\
    By default, plugins are loaded from current directory where primary executable is started (Working directory is not used by default because windows service working directory is: %WinDir%\System32). If required with the help of interface IPluginProvider we can write our own plugin, witch will load plugins from different source.
3.  Ability to use convenient user interface (SDI/MDI/Dialog interface).\
    For user with maximum rights and limitless forms may be convenient to use MDI based application. For user with minimum rights and with 1,2 controls MDI interface will be less convenient. For developer, who is using IDE Visual Studio may be convenient to use VS IDE to host required controls.
4.  Reusable codebase, witch can be used not only in WinForms applications, but also in ASP.NET or Win/Web services.
    For example plugins like autorun application on system starts, single instance plugin, settings configuration plugin, runtime compilation plugin, timer plugin or plugins that can capture some external events may be loaded from the central storage for all applications.

##  Concept

Unlike the common extension architecture, the basis for SAL architecture is the plugin with inherit IPluginKernel interface. At the same time, the module itself can extend or use other modules.

Result:

1.  Modules can be used out of the box without additional recompilation procedures.
    - From the code of one module, you can access the code of another module.
    - Partially change one plugin, all dependent plugins will get new version without recompilation or without manually changing the partition bindingRedirect in .config file.
    - Save and restore all controls between closing and opening application.
2.  Ease of integration
3.  Ease to transfer. UserControl classes are used as UI components for WinForms applications (The use of the Form class as the basis does not allow some hosts. For example EnvDTE). If necessary, the code can be returned to the standard application at no extra cost.

##  Architecture

The running application looks like this:

1.  Application (Supported interfaces: MDI/DialogBased/EnvDTE/Windows Service/ASP.NET)
2.  SAL host, who knows how to communicate with the application and loads plugins.
3.  Plugins, which do not know what application they are running in, but who can communicate with the application through the SAL host.

##  Application

Application can be a SAL host as in case of Flatbed.MDI, Flatbed.Dialog. And a separate plugin, as in the case of Flatbed.EnvDTE.
###  SAL host

- After loading all found plugins into memory from current application folder by default, application is looking for plugins of 2 types: settings provider (ISettingsProvider) and plugins provider (IPluginProvider). If different settings or plugins providers are found then default provider will became parent provider and new provider will be used as default provider.
    - ISettingsProvider allows to load plugin settings from different sources. In the host itself is implemented default provider that loading data from XML file.
    - IPluginProvider allows to load plugins and update plugins from different sources. In the host itself is implemented default provider that loading plugins from current application folder.
- After providers are correctly attached and all plugins and settings are loaded from different sources, application is searching for Kernel plugin. There can be multiple Kernel plugins and each of them can provide BLL or DAL. Also Kernel plugin can influence to UI (in case of WinForms applications) and can cancel loading other plugins in the runtime. For example, if plugin is loaded witch should not be in the system, then further loading can be cancelled.
- After all plugins are loaded and all Kernel plugins are initialized, all other plugins starting initialization process. Each futher plugin Each subsequent plugin, if necessary, searches the list of Kernel plugins for the plugin to which it refers (Reference). If the plugin is unified, then it does not need a link to the Kernel plugin.

### Plugins
If plugin is referenced only to interfaces of the SAL.Flatbed assembly, then it can't use UI and can work in any environment. If plugin is referenced to upper level assembly, for example SAL.Windows (Which referenced to SAL.Flatbed), then plugin will need WinForms UI to implement the full functionality.

SAL Host can provide additional set of interfaces that inherit from the minimal interface base on the host itself. For example to get access to additional features of Flatbed.EnvDTE host, we need to cast IHost interface to IHostAddIn interface, which described in assembly SAL.EnvDTE.

If required, one plugin can invoke other plugin. But if required plugin is not loaded, then part of logic of UI will be unavailable and there will be no exception about the same situation.
## Assembly description

   - SAL.Flatbed.dll — Root assembly, that contains minimal set of interfaces to implement plugins and hosts.
   - SAL.Windows.dll — Assembly to develop components for WinForms applications.

## Description of interaction
After all plugins are loaded, for each plugin method Boolean OnConnection(ConnectMode mode) is called.

Interaction between plugins a based on public methods properties and events.

Interaction between plugin and host is available through IHost argument that can be received from plugin constructor method. For example if plugin can interact with WinForms host, then we can try to cast IHost to IHostWindows, which will add additional features related to WinForms.

## Interaction examples
Receive and load plugin settings. (With inheritance from IPluginSettings<T> interface).

    public class Plugin : IPlugin, IPluginSettings<PluginSettings>
    {
        ...
        private PluginSettings _settings;
        public PluginSettings Settings
        {
            get
            {
                if(this._settings == null)
                {
                    this._settings = new PluginSettings(this);
                    this.Host.Plugins.Settings(this).LoadAssemblyParameters(this._settings);
                }
                return this._settings;
            }
        }
        Object IPluginSettings.Settings { get { return this.Settings; } }
        ...
    }

Loading big object from settings (Example: DataSet)

    using(Stream stream = this.Plugin.Host.Plugins.Settings(this.Plugin).LoadAssemblyBlob("DataSet.xml"))
        if(stream != null)
            base.DataSet.ReadXml(stream);

Saving big object to settings. (DataSet for example)

    using(MemoryStream stream = new MemoryStream())
    {
        base.DataSet.WriteXml(stream);
        this.Plugin.Host.Plugins.Settings(this).SaveAssemblyBlob("DataSet.xml", stream);
    }

Adding elements to application that supports SAL.Windows interfaces.

    internal IHost Host { get; private set; }//It's allowed to launch the plugin with any host, and at startup - the plugin decides how to start

    public Plugin(IHost host){
        this.Host = host;
    }
    ...
    IHostWindows host = this.Host as IHostWindows;
    if(host != null)
    {
        IMenuItem menuTools = host.MainMenu.FindMenuItem("Tools");
        if(menuTools != null)
        {
            this.RdpClientMenu = menuTools.Create("RDP Client");
            this.RdpClientMenu.Name = "tsmiToolsRdpClient";
            this.RdpClientMenu.Click += new EventHandler(RdpClientMenu_Click);
            menuTools.Items.Insert(0, this.RdpClientMenu);
            return true;
        }
    }

Opening window (We must inherit from UserControl class)

    internal IHostWindows HostWindows { get; private set; }

    public Plugin(IHostWindows host) {
        this.HostWindows = host;
    }//Creating an instance of a plugin is only possible in a host that implements the IHostWindows interface
    ...
    IWindow wnd = this.HostWindows.Windows.CreateWindow(this, typeof(PanelRdpClient).ToString(), false, DockState.DockLeft);

Receiving objects IWindow and IPlugin in the opened window.

    public partial class PanelRdpClient : UserControl
    {
        private IWindow Window { get { return (IWindow)base.Parent; } }
        private PluginRdp Plugin { get { return (PluginWindows)this.Window.Plugin; } }
        ...
        protected override void OnCreateControl()
        {
            this.Window.Caption = "I want cookie";

            base.OnCreateControl();
        }
        ...
    }