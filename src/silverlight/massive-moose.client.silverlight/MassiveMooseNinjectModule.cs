using Ninject;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace massive_moose.client.silverlight
{
    public class MassiveMooseNinjectModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<IMassiveMooseService>().To<MassiveMooseService>();
        }
    }

    public class ViewModelLocator
    {
        public static readonly IKernel Kernel;

        static ViewModelLocator()
        {
            if (Kernel == null)
            {
                Kernel = new StandardKernel(new MassiveMooseNinjectModule());
            }
        }
    }
}
