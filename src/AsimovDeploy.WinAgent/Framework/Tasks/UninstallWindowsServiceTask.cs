using System;
using System.Collections.Generic;
using System.IO;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.Units;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class UninstallWindowsServiceTask : AsimovTask
    {
        private readonly InstallableConfig _installableConfig;
        private readonly DeployUnit _unit;
        private readonly NodeFront _nodefront = new NodeFront();
        public UninstallWindowsServiceTask(InstallableConfig installableInstallableConfig, DeployUnit unit)
        {
            _installableConfig = installableInstallableConfig;
            _unit = unit;
        }

        protected override void Execute()
        {
            if (string.IsNullOrEmpty(_installableConfig.TargetPath))
                _installableConfig.TargetPath = Path.Combine(@"\Services", _unit.Name);
            if (!string.IsNullOrEmpty(_installableConfig.Uninstall))
                UnInstallUsingCommandFromConfig();
            else if (_installableConfig.InstallType.Equals("NServiceBus", StringComparison.InvariantCultureIgnoreCase))
                UnInstallNServiceBusHandler();
            else if (_installableConfig.InstallType.Equals("TopShelf", StringComparison.InvariantCultureIgnoreCase))
                UnInstallTopShelfService();

        }

        private void UnInstallNServiceBusHandler()
        {
            var exePath = $"{_installableConfig.TargetPath}\\NServiceBus.Host.exe";

            var args = new List<string>
            {
                "/uninstall",
                "/servicename:" + _unit.Name,
                "-f",
               _installableConfig.TargetPath
            };
            ProcessUtil.ExecuteCommand(exePath, args.ToArray(), Log);
            //"{0}NServiceBus.Host.exe /uninstall /serviceName:$ProcessServiceName.$ApplicationName" -f $HandlerDirectory

        }

        private void UnInstallTopShelfService()
        {
            var exePath = $"{_installableConfig.TargetPath}\\{_installableConfig.AssemblyName ?? _unit.Name}.exe";
            ProcessUtil.ExecuteCommand(exePath, new[] { "uninstall" }, Log);

        }

        private void UnInstallUsingCommandFromConfig()
        {
            ProcessUtil.ExecutePowershellScript(
                _installableConfig.TargetPath, //TODO: We may want to use the location of the service in the future
                _installableConfig.Uninstall,
                new ParameterValues(new Dictionary<string, object>()), Log);

            _nodefront.Notify(new UnitStatusChangedEvent(_unit.Name, _unit.GetUnitInfo().Status));
        }
    }
}