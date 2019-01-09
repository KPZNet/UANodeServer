using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaServer;
using UnifiedAutomation.UaSchema;


namespace UANodeServer
{
    class Program
    {
        internal class GettingStartedServerManager : ServerManager
        {
            /// [OnRootNodeManagerStarted]
            protected override void OnRootNodeManagerStarted(RootNodeManager nodeManager)
            {
                Console.WriteLine("Creating Node Managers...");

                KNodeManager knm = new KNodeManager(this);
                knm.Startup();
            }
            /// [OnRootNodeManagerStarted]
        }

        internal class KNodeManager : BaseNodeManager
        {

            #region Public Properties

            public ushort InstanceNamespaceIndex { get; set; }
            public ushort TypeNamespaceIndex { get; set; }
            #endregion

            uint totNodes = 0;
            static uint newNodeId = 4000;


            void AddVariables(NodeId rootNode, int totVarSets)
            {
                for (int i = 1; i <= totVarSets; i++)
                {
                    totNodes++;
                    string nName = "Property" + "_" + i.ToString();
                    newNodeId++;
                    var variableSettings = new CreateVariableSettings()
                    {
                        BrowseName = nName,
                        DisplayName = nName,
                        ParentNodeId = rootNode,
                        ParentAsOwner = true,
                        ReferenceTypeId = UnifiedAutomation.UaBase.ReferenceTypeIds.HasProperty,
                        TypeDefinitionId = UnifiedAutomation.UaBase.VariableTypeIds.PropertyType,
                        DataType = UnifiedAutomation.UaBase.DataTypeIds.Argument,
                        ModellingRuleId = UnifiedAutomation.UaBase.ObjectIds.ModellingRule_Mandatory,
                        RequestedNodeId = new NodeId(newNodeId, InstanceNamespaceIndex),
                        Value = new Variant(10.1)
                    };
                    CreateVariable(Server.DefaultRequestContext, variableSettings);
                }
                for (int i = 1; i <= totVarSets; i++)
                {
                    totNodes++;
                    string nName = "Variable" + "_" + i.ToString();
                    newNodeId++;
                    var variableSettings = new CreateVariableSettings()
                    {
                        BrowseName = nName,
                        DisplayName = nName,
                        ParentNodeId = rootNode,
                        ParentAsOwner = true,
                        ReferenceTypeId = UnifiedAutomation.UaBase.ReferenceTypeIds.HasComponent,
                        TypeDefinitionId = UnifiedAutomation.UaBase.VariableTypeIds.BaseVariableType,
                        DataType = UnifiedAutomation.UaBase.DataTypeIds.Argument,
                        ModellingRuleId = UnifiedAutomation.UaBase.ObjectIds.ModellingRule_Mandatory,
                        RequestedNodeId = new NodeId(newNodeId, InstanceNamespaceIndex),
                        Value = new Variant(10.1)
                    };
                    CreateVariable(Server.DefaultRequestContext, variableSettings);
                }
            }

            void AddRootNode(NodeId rootNode, string baseName, int[] tNodes, int[] tProps, int currDepth)
            {
                currDepth = currDepth + 1;

                if (currDepth <= tNodes.Length)
                {
                    int nodesAtCurrentDepth = tNodes[currDepth - 1];
                    for (int i = 1; i <= nodesAtCurrentDepth; i++)
                    {
                        string nName = baseName + "_" + currDepth.ToString() + "_" + i.ToString();
                        newNodeId++;

                        CreateObjectSettings settings = new CreateObjectSettings()
                        {
                            ParentNodeId = rootNode,
                            ReferenceTypeId = ReferenceTypeIds.HasComponent,
                            RequestedNodeId = new NodeId(newNodeId, InstanceNamespaceIndex),
                            BrowseName = new QualifiedName(nName, InstanceNamespaceIndex),
                            TypeDefinitionId = ObjectTypeIds.BaseObjectType
                        };
                        var pN = CreateObject(Server.DefaultRequestContext, settings);
                        totNodes++;
                        AddVariables(pN.NodeId, tProps[currDepth - 1]);
                        AddRootNode(pN.NodeId, baseName, tNodes, tProps, currDepth);

                    }
                }
            }

            public override void Startup()
            {
                try
                {
                    Console.WriteLine("Starting UANodeServer.");

                    base.Startup();

                    AddNamespaceUri("http://yourorganisation.com/lesson01/");

                    // save the namespaces used by this node manager.
                    InstanceNamespaceIndex = AddNamespaceUri("http://kpznet.com/KNodeServer/");

                    // Create a Folder for Controllers
                    CreateObjectSettings settings = new CreateObjectSettings()
                    {
                        ParentNodeId = ObjectIds.ObjectsFolder,
                        ReferenceTypeId = ReferenceTypeIds.Organizes,
                        RequestedNodeId = new NodeId("GRoot", InstanceNamespaceIndex),
                        BrowseName = new QualifiedName("GRoot", InstanceNamespaceIndex),
                        TypeDefinitionId = ObjectTypeIds.BaseObjectType
                    };
                    var gRoot = CreateObject(Server.DefaultRequestContext, settings);

                    int[] tNodes = new int[] { 2, 8, 6, 4 };
                    int[] tVars = new int[] { 5, 5, 5, 5 };
                    AddRootNode(gRoot.NodeId, "NODE", tNodes, tVars, 0);

                    totNodes++;
                    string nName = "Total Nodes";
                    newNodeId++;
                    var variableSettings = new CreateVariableSettings()
                    {
                        BrowseName = nName,
                        DisplayName = nName,
                        ParentNodeId = gRoot.NodeId,
                        ParentAsOwner = true,
                        ReferenceTypeId = UnifiedAutomation.UaBase.ReferenceTypeIds.HasProperty,
                        TypeDefinitionId = UnifiedAutomation.UaBase.VariableTypeIds.PropertyType,
                        DataType = UnifiedAutomation.UaBase.DataTypeIds.Argument,
                        ModellingRuleId = UnifiedAutomation.UaBase.ObjectIds.ModellingRule_Mandatory,
                        RequestedNodeId = new NodeId(newNodeId, InstanceNamespaceIndex),
                        Value = new Variant(totNodes)
                    };
                    CreateVariable(Server.DefaultRequestContext, variableSettings);

                    Console.WriteLine("Total Nodes = " + totNodes);

                }
                /// [Set Namespace URI]
                catch (Exception e)
                {
                    Console.WriteLine("Failed to start UANodeManager " + e.Message);
                }
            }

            /// <summary>
            /// Called when the node manager is stopped.
            /// </summary>
            public override void Shutdown()
            {
                try
                {
                    Console.WriteLine("Stopping UANodeManager.");

                    // TBD 

                    base.Shutdown();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to stop UANodeManager " + e.Message);
                }
            }

            #region Constructor
            /// <summary>
            /// Initializes a new instance of the class.
            /// </summary>
            public KNodeManager(ServerManager server) : base(server)
            {
            }
            #endregion

            #region IDisposable
            /// <summary>
            /// An overrideable version of the Dispose.
            /// </summary>
            /// <param name="disposing"></param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // TBD
                }
            }
            #endregion

            #region Private Methods
            #endregion

            #region Private Fields
            #endregion
        }


        static void Main(string[] args)
        {
            try
            {
                // The license file must be loaded from an embedded resource.
                ApplicationLicenseManager.AddProcessLicenses(System.Reflection.Assembly.GetExecutingAssembly(), "License.lic");

                // Start the server.
                Console.WriteLine("Starting Server.");
                GettingStartedServerManager server = new GettingStartedServerManager();
                //***********************************************************************
                // The following function can be called to configure the server from code
                // This will disable the configuration settings from app.config file
                //ConfigureOpcUaApplicationFromCode();
                //***********************************************************************
                ApplicationInstance.Default.AutoCreateCertificate = true;
                ApplicationInstance.Default.Start(server, null, server);

                // Print endpoints for information.
                PrintEndpoints(server);

                // Block until the server exits.
                Console.WriteLine("Press <enter> to exit the program.");
                Console.ReadLine();

                // Stop the server.
                Console.WriteLine("Stopping Server.");
                server.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
                Console.WriteLine("Press <enter> to exit the program.");
                Console.ReadLine();
            }
        }
        /// [Main]

        /// <summary>
        /// Prints the available EndpointDescriptions to the command line.
        /// </summary>
        /// <param name="server"></param>
        /// [PrintEndpoints]
        static void PrintEndpoints(GettingStartedServerManager server)
        {
            // print the endpoints.
            Console.WriteLine(string.Empty);
            Console.WriteLine("Listening at the following endpoints:");

            foreach (EndpointDescription endpoint in ApplicationInstance.Default.Endpoints)
            {
                StatusCode error = server.Application.GetEndpointStatus(endpoint);
                Console.WriteLine("   {0}: Status={1}", endpoint, error.ToString(true));
            }

            Console.WriteLine(string.Empty);
        }
        /// [PrintEndpoints]

        /// [Configuration Options 2]
        static void ConfigureOpcUaApplicationFromCode()
        {
            // fill in the application settings in code
            // The configuration settings are typically provided by another module
            // of the application or loaded from a data base. In this example the
            // settings are hardcoded
            SecuredApplication application = new SecuredApplication();

            // ***********************************************************************
            // standard configuration options

            // general application identification settings
            application.ApplicationName = "UnifiedAutomation GettingStartedServer";
            application.ApplicationUri = "urn:localhost:UnifiedAutomation:GettingStartedServer";
            application.ApplicationType = UnifiedAutomation.UaSchema.ApplicationType.Server_0;
            application.ProductName = "UnifiedAutomation GettingStartedServer";

            // configure certificate stores
            application.ApplicationCertificate = new UnifiedAutomation.UaSchema.CertificateIdentifier();
            application.ApplicationCertificate.StoreType = "Directory";
            application.ApplicationCertificate.StorePath = @"%CommonApplicationData%\unifiedautomation\UaSdkNet\pki\own";
            application.ApplicationCertificate.SubjectName = "CN=GettingStartedServer/O=UnifiedAutomation/DC=localhost";

            application.TrustedCertificateStore = new UnifiedAutomation.UaSchema.CertificateStoreIdentifier();
            application.TrustedCertificateStore.StoreType = "Directory";
            application.TrustedCertificateStore.StorePath = @"%CommonApplicationData%\unifiedautomation\UaSdkNet\pki\trusted";

            application.IssuerCertificateStore = new UnifiedAutomation.UaSchema.CertificateStoreIdentifier();
            application.IssuerCertificateStore.StoreType = "Directory";
            application.IssuerCertificateStore.StorePath = @"%CommonApplicationData%\unifiedautomation\UaSdkNet\pki\issuers";

            application.RejectedCertificatesStore = new UnifiedAutomation.UaSchema.CertificateStoreIdentifier();
            application.RejectedCertificatesStore.StoreType = "Directory";
            application.RejectedCertificatesStore.StorePath = @"%CommonApplicationData%\unifiedautomation\UaSdkNet\pki\rejected";

            // configure endpoints
            application.BaseAddresses = new UnifiedAutomation.UaSchema.ListOfBaseAddresses();
            application.BaseAddresses.Add("opc.tcp://localhost:48030");

            application.SecurityProfiles = new ListOfSecurityProfiles();
            application.SecurityProfiles.Add(new SecurityProfile() { ProfileUri = SecurityProfiles.Basic256Sha256, Enabled = true });
            application.SecurityProfiles.Add(new SecurityProfile() { ProfileUri = SecurityProfiles.None, Enabled = true });
            // ***********************************************************************

            // ***********************************************************************
            // extended configuration options

            // trace settings
            TraceSettings trace = new TraceSettings();

            trace.MasterTraceEnabled = true;
            trace.DefaultTraceLevel = UnifiedAutomation.UaSchema.TraceLevel.Info;
            trace.TraceFile = @"%CommonApplicationData%\unifiedautomation\logs\ConfigurationServer.log.txt";
            trace.MaxLogFileBackups = 3;

            trace.ModuleSettings = new ModuleTraceSettings[]
                {
                    new ModuleTraceSettings() { ModuleName = "UnifiedAutomation.Stack", TraceEnabled = true },
                    new ModuleTraceSettings() { ModuleName = "UnifiedAutomation.Server", TraceEnabled = true },
                };

            application.Set<TraceSettings>(trace);

            // Installation settings
            InstallationSettings installation = new InstallationSettings();

            installation.GenerateCertificateIfNone = true;
            installation.DeleteCertificateOnUninstall = true;

            application.Set<InstallationSettings>(installation);
            // ***********************************************************************

            // set the configuration for the application (must be called before start to have any effect).
            // these settings are discarded if the /configFile flag is specified on the command line.
            ApplicationInstance.Default.SetApplicationSettings(application);
        }
    }
}
