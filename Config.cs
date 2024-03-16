using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace NWinLog {
    internal class Config {

        private static string _AppSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string AppSettingsPath {
            set {
                _AppSettingsPath = value;
            }
            get {
                return _AppSettingsPath;
            }
        }
        public static string _CompanyName = "";
        public static string CompanyName {
            set {
                _CompanyName = value;
            }
            get {
                return _CompanyName;
            }
        }

        public static string _ProductName = "";
        public static string ProductName {
            set {
                _ProductName = value;
            }
            get {
                return _ProductName;
            }
        }

        private static string _AppSysFolderPath =
            String.Format(@"{0}\{1}\{2}",
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    CompanyName,
                    ProductName
                    );


        internal static bool _IsUserConfigSettings = false;

        private static string _AppConfigPath =
            String.Format(@"{0}\{1}\{2}\config",
                _AppSettingsPath,
                CompanyName,
                ProductName
            );

        public static string AppSysFolderPath {
            get {
                return _AppSysFolderPath;
            }
        }

        public static string AppConfigPath {
            set {
                _AppConfigPath = value;
                _IsUserConfigSettings = true;
            }

            get {
                if(_IsUserConfigSettings)
                    return _AppConfigPath;
                _AppConfigPath =
                            String.Format(@"{0}\{1}\{2}\config",
                                    _AppSettingsPath,
                                    CompanyName,
                                    ProductName
                                    );
                return _AppConfigPath;
            }
        }
    }
}
