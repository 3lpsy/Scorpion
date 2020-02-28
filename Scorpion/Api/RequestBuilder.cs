using System.Collections.Generic;
using System.Threading.Tasks;
using Covenant.API;
using Covenant.API.Models;
using Scorpion.Exceptions;
using Microsoft.Rest;

namespace Scorpion.Api
{
  public class RequestBuilder
  {
    public CovenantAPI Api { get; set; }
    public RequestBuilder(CovenantAPI api)
    {
      Api = api;
    }

    public async Task<IList<CovenantUser>> GetUsers()
    {
      try {
        return await MakeGetUsers();
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }

    public async Task<IList<CovenantUser>> MakeGetUsers()
    {
      var result = await Api.ApiUsersGetWithHttpMessagesAsync();
      return result.Body;
    }

    public async Task<bool> DeleteUserById(string userId)
    {
      try {
        return await MakeDeleteUserById(userId);
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }


    public async Task<bool> MakeDeleteUserById(string userId)
    {
      var result = await Api.ApiUsersByIdDeleteWithHttpMessagesAsync(userId);
      return true;
    }

    public async Task<CovenantUser> CreateUser(string newUserName, string newPassword)
    {
      try {
        return await MakeCreateUser(newUserName, newPassword);
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }

    public async Task<CovenantUser> MakeCreateUser(string newUserName, string newPassword)
    {
      var userData = new CovenantUserLogin(newUserName, newPassword);
      var result = await Api.ApiUsersPostWithHttpMessagesAsync(userData);
      return result.Body;
    }


    public async Task<IList<IdentityRole>> GetRoles()
    {
      try {
        return await MakeGetRoles();
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }

    public async Task<IList<IdentityRole>> MakeGetRoles()
    {
      var result = await Api.ApiRolesGetWithHttpMessagesAsync();
      return result.Body;
    }

    public async Task<bool> AddUserRole(string userId, string roleId)
    {
      try {
        return await MakeAddUserRole(userId, roleId);
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }

    public async Task<bool> MakeAddUserRole(string userId, string roleId)
    {
      var result = await Api.ApiUsersByIdRolesByRidPostWithHttpMessagesAsync(userId, roleId);
      IdentityUserRoleString roleString = result.Body;
      return true;
    }

    public async Task<IList<Listener>> GetListeners()
    {
      try {
        return await MakeGetListeners();
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }

    public async Task<IList<Listener>> MakeGetListeners()
    {
      var result = await Api.ApiListenersGetWithHttpMessagesAsync();
      return result.Body;
    }

    public async Task<IList<ListenerType>> GetListenerTypes()
    {
      try {
        return await MakeGetListenerTypes();
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }

    public async Task<IList<ListenerType>> MakeGetListenerTypes()
    {
      var result = await Api.ApiListenersTypesGetWithHttpMessagesAsync();
      return result.Body;
    }

    public async Task<ListenerType> GetListenerTypeByName(string listenerTypeName)
    {
      var listenerTypes = await GetListenerTypes();

      foreach (ListenerType listenerType in listenerTypes) {
        if (listenerType.Name.ToLower() == listenerTypeName.ToLower()) {
          return listenerType;
        }
      }
      throw new AppException($"Received API Error: No Listener Type found for name.");
    }

    public async Task<IList<Profile>> GetProfiles()
    {
      try {
        return await MakeGetProfiles();
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }

    public async Task<IList<Profile>> MakeGetProfiles()
    {
      var result = await Api.ApiProfilesGetWithHttpMessagesAsync();
      return result.Body;
    }


    public async Task<Profile> GetProfileByName(string profileName)
    {
      var profiles = await GetProfiles();

      foreach (Profile profile in profiles) {
        if (profile.Name.ToLower() == profileName.ToLower()) {
          return profile;
        }
      }
      throw new AppException($"Received API Error: No Profile found for name.");
    }

    public async Task<IList<ImplantTemplate>> GetImplantTemplates()
    {
      try {
        return await MakeGetImplantTemplates();
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }

    public async Task<IList<ImplantTemplate>> MakeGetImplantTemplates()
    {
      var result = await Api.ApiImplanttemplatesGetWithHttpMessagesAsync();
      return result.Body;
    }

    public async Task<ImplantTemplate> GetImplantTemplateByName(string templateName)
    {
      var templates = await GetImplantTemplates();

      foreach (ImplantTemplate template in templates) {
        if (template.Name.ToLower() == templateName.ToLower()) {
          return template;
        }
      }
      throw new AppException($"Received API Error: No Profile found for name.");
    }

    public async Task<HttpListener> CreateHttpListener(HttpListener httpListener)
    {
      try {
        return await MakeCreateHttpListener(httpListener);
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }

    public async Task<HttpListener> MakeCreateHttpListener(HttpListener httpListener)
    {
      var result = await Api.ApiListenersHttpPostWithHttpMessagesAsync(httpListener);
      return result.Body;
    }

    public async Task<bool> DeleteListenerById(int listenerId)
    {
      try {
        return await MakeDeleteListenerById(listenerId);
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }


    public async Task<bool> MakeDeleteListenerById(int listenerId)
    {
      await Api.ApiListenersByIdDeleteAsync(listenerId);
      return true;
    }
    public async Task<BinaryLauncher> CreateBinaryLauncher(BinaryLauncher launcher)
    {
      try {
        return await MakeCreateBinaryLauncher(launcher);
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }


    public async Task<BinaryLauncher> MakeCreateBinaryLauncher(BinaryLauncher launcher)
    {
      var result = await Api.ApiLaunchersBinaryPutWithHttpMessagesAsync(launcher);
      return result.Body;

    }

    public async Task<HostedFile> CreateHostedFile(int listenerId, HostedFile hostedFile)
    {
      try {
        return await MakeCreateHostedFile(listenerId, hostedFile);
      } catch (HttpOperationException ex) {
        throw new AppException($"Received API Error: {ex.Message}");
      }
    }


    public async Task<HostedFile> MakeCreateHostedFile(int listenerId, HostedFile hostedFile)
    {
      var result = await Api.ApiListenersByIdHostedfilesPostWithHttpMessagesAsync(listenerId, hostedFile);
      return result.Body;
    }


  }
}