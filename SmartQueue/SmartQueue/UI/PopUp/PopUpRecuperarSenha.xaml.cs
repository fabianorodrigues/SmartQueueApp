﻿using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SmartQueue.Controller;
using SmartQueue.Utils;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartQueue.UI.PopUp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PopUpRecuperarSenha : PopupPage
    {
        private UsuarioController controller;

        public PopUpRecuperarSenha ()
		{
			InitializeComponent ();
            controller = new UsuarioController();
		}

        public async void Recuperar()
        {
            try
            {
                if (!Aplicacao.EmailValido(txtEmail.Text))
                    await DisplayAlert("E-mail inválido", "Digite um e-mail válido.", "Ok");
                else if (await controller.RecuperarSenhaEmail(txtEmail.Text))
                {
                    await DisplayAlert("Senha Alterada com sucesso.", "Nova senha encaminhada para o e-mail cdastrado.", "Ok");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Atenção", ex.Message, "Ok");
            }
        }

        private void Recuperar_Clicked(object sender, System.EventArgs e)
        {
            Recuperar();
        }

        private void txt_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            Aplicacao.MostrarLabel(true, ((Entry)sender));
        }

        private void txt_Unfocused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            if (((Entry)sender).Text == string.Empty || ((Entry)sender).Text == null)
                Aplicacao.MostrarLabel(false, (Entry)sender);
        }

        private async void Cancelar_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync(true);
        }
    }
}