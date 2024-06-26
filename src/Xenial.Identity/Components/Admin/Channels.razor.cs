﻿using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;

using Microsoft.AspNetCore.Components;

using Xenial.Identity.Channels;
using Xenial.Identity.Models;

namespace Xenial.Identity.Components.Admin;

public partial class Channels
{
    private async Task Edit(XpoCommunicationChannel channel)
    {
        var registration = ChannelRegistry.GetChannelRegistration(channel.ChannelType, channel.ChannelProviderType);
        var channelSerivce = ChannelRegistry.GetChannel(channel.ChannelType, channel.ChannelProviderType);
        var settings = channelSerivce.DeserializeChannelSettings(channel.ChannelSettings);

        using var childUow = UnitOfWork.BeginNestedUnitOfWork();

        var dialog = DialogService.Show<ChannelDialog>("Add Channel", new MudBlazor.DialogParameters
        {
            [nameof(ChannelDialog.UnitOfWork)] = childUow,
            [nameof(ChannelDialog.Channel)] = childUow.GetObjectByKey<XpoCommunicationChannel>(channel.Id),
            [nameof(ChannelDialog.Registration)] = registration,
            [nameof(ChannelDialog.Settings)] = settings,
        }, new MudBlazor.DialogOptions
        {
            MaxWidth = MudBlazor.MaxWidth.Small,
            FullWidth = true,
            Position = MudBlazor.DialogPosition.TopCenter,
            NoHeader = false,
            CloseButton = true,
            CloseOnEscapeKey = true
        });

        var result = await dialog.GetReturnValueAsync<bool?>();
        if (result == true)
        {
            channel.ChannelSettings = channelSerivce.SerializeChannelSettings(settings);
            await childUow.SaveAsync(channel);
            await childUow.CommitChangesAsync();
            await UnitOfWork.CommitChangesAsync();
        }
        else
        {
            childUow.DropChanges();
        }

        await table.ReloadServerData();
    }

    private async Task Add(ICommunicationChannelRegistration registration)
    {
        var channelSerivce = ChannelRegistry.GetChannel(registration);
        var settings = channelSerivce.CreateChannelSettings();

        using var childUow = UnitOfWork.BeginNestedUnitOfWork();
        var channel = new XpoCommunicationChannel(childUow)
        {
            ChannelProviderType = registration.ProviderType,
            ChannelDisplayName = registration.DisplayName,
            ChannelType = registration.Type
        };

        var dialog = DialogService.Show<ChannelDialog>("Add Channel", new MudBlazor.DialogParameters
        {
            [nameof(ChannelDialog.UnitOfWork)] = childUow,
            [nameof(ChannelDialog.Channel)] = channel,
            [nameof(ChannelDialog.Registration)] = registration,
            [nameof(ChannelDialog.Settings)] = settings,
        }, new MudBlazor.DialogOptions
        {
            MaxWidth = MudBlazor.MaxWidth.Small,
            FullWidth = true,
            Position = MudBlazor.DialogPosition.TopCenter,
            NoHeader = false,
            CloseButton = true,
            CloseOnEscapeKey = true
        });

        var result = await dialog.GetReturnValueAsync<bool?>();
        if (result == true)
        {
            channel.ChannelSettings = channelSerivce.SerializeChannelSettings(settings);
            await childUow.SaveAsync(channel);
            await childUow.CommitChangesAsync();
            await UnitOfWork.CommitChangesAsync();
        }
        else
        {
            childUow.DropChanges();
        }

        await table.ReloadServerData();
    }
    private async Task Delete(XpoCommunicationChannel channel)
    {
        var delete = await DialogService.ShowMessageBox("Delete API", (MarkupString)$"""
            <ul>
                <li>
                    Do your really want to delete the API?
                </li>
                <li>
                    <em>{channel.ChannelProviderType}</em>
                </li>
                <li>
                    <strong>This operation can <em>not</em> be undone!</strong>
                </li>
            </ul>
        """, yesText: "Delete!", cancelText: "Cancel");

        if (delete == true)
        {
            try
            {
                await UnitOfWork.DeleteAsync(channel);
                await UnitOfWork.CommitChangesAsync();
                _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            API was successfully deleted!
                        </li>
                        <li>
                            <em>{channel.ChannelProviderType}</em>
                        </li>
                    </ul>
                    """, MudBlazor.Severity.Success);
                openDrawer = false;
            }
            catch (Exception ex)
            {
                var errors = ex.Message;

                _ = Snackbar.Add($"""
                    <ul>
                        <li>
                            There was an error when deleting the API!
                        </li>
                        <li>
                            <em>{channel.ChannelProviderType}</em>
                        </li>
                        {errors}
                    </ul>
                    """, MudBlazor.Severity.Error);
            }
        }

        await table.ReloadServerData();
    }
}
