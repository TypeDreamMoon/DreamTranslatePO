using System.Collections.Specialized;
using Microsoft.Windows.AppNotifications.Builder;

namespace DreamTranslatePO.Contracts.Services;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    bool Show(AppNotificationBuilder builder);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();
}
