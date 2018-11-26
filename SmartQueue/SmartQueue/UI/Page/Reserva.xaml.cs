﻿using Plugin.LocalNotifications;
using SmartQueue.Controller;
using SmartQueue.DAL;
using SmartQueue.Model;
using SmartQueue.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmartQueue.UI.Page
{
    
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Reserva : ContentPage
	{
        private ReservaController controller;
        private int segundos;
        private int minutos;
        private int horas;
        private bool reservaCancelada;
        private Dictionary<string, int> dicListaItensPendentes;

        public Reserva ()
		{
			InitializeComponent();
            controller = new ReservaController();

            ConsultarTempo();             

        }

        private async void CarregaDados()
        {
            if(lvPedidosPendentes.ItemsSource == null)
            {
                List<ItemPedido> itens = controller.ItensPedidosPendentes();

                if (itens.Count > 0)
                {
                    List<Produto> produtos = await new ProdutoController().ListarProdutos();

                    dicListaItensPendentes = new Dictionary<string, int>();

                    foreach (var item in itens)
                    {
                        var produto = produtos.First(x => x.Id == item.ProdutoId);
                        dicListaItensPendentes.Add(produto.Nome, item.Quantidade);
                    }

                    lvPedidosPendentes.ItemsSource = dicListaItensPendentes;

                    layoutPedidosPendentes.IsVisible = true;
                }
                else
                    layoutPedidosPendentes.IsVisible = false;

                
            }
            
        }

        public async void ConsultarTempo()
        {
            try
            {
                string statusReserva = new StorageReserva().Consultar().Status;

                if (statusReserva == "Em Uso")
                {
                    ComponentesAtivarMesa(true);
                    ComponentesTempo(false);

                    var menuReserva = this.Parent as TabbedPage;
                    menuReserva.CurrentPage = menuReserva.Children[2];
                    menuReserva.Children.RemoveAt(0);
                }
                else
                {
                    TimeSpan tempo = await controller.ConsultarTempo();
                    horas = tempo.Hours;
                    minutos = tempo.Minutes;
                    segundos = tempo.Seconds;

                    //horas = 0;
                    //minutos = 0;
                    //segundos = 10;

                    LiberarMesa();
                }
                
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "Ok");
                await Navigation.PopAsync();
            }
            
        }

        public void LiberarMesa()
        {  
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (reservaCancelada)
                    return false;
                else if (segundos == 0)
                {
                    if (minutos == 0)
                    {
                        if (horas == 0)
                        {
                            //CrossLocalNotifications.Current.Show("Mesa Liberada.", string.Format("Ao chegar na mesa realize checkin com o número e senha da mesa."));
                            ComponentesAtivarMesa(true);
                            ComponentesTempo(false);
                            return false;
                        }
                        else
                        {
                            horas -= 1;
                            minutos = 60;
                        }
                    }
                    else
                    {
                        minutos -= 1;
                        segundos = 60;
                    }

                }
                else
                    segundos -= 1;

                lblTempoLiberarMesa.Text = string.Format("{2}:{1}:{0}", segundos.ToString("00"), minutos.ToString("00"), horas.ToString("00"));
                return true;
            });
            
        }

        private async void CancelarReserva()
        {
            try
            {
                var sair = await DisplayAlert("Cancelar Reserva", "Tem certeza que deseja cancelar a reserva?", "Sim", "Não");

                if (sair)
                {
                    if(await controller.CancelarMesa())
                    {
                        reservaCancelada = true;
                        await Navigation.PopAsync(true);
                    }
                        
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
            
                
        }

        private async Task<bool> ValidaAtivarReserva()
        {
            if(string.IsNullOrEmpty(txtNmrMesa.Text) || string.IsNullOrEmpty(txtSenhaMesa.Text))
            {
                await DisplayAlert("Atenção", "Digite a senha e o número da mesa corretamente.", "OK");
                return false;
            }
            else if (txtSenhaMesa.Text.Length < 6)
            {
                await DisplayAlert("Senha inválida", "A senha deve ter ao menos 6 dígitos.", "Ok");
                return false;
            }

            return true;
        }

        private async void AtivarReserva()
        {         
            try
            {
                if (await controller.AtivarReserva(int.Parse(txtNmrMesa.Text), txtSenhaMesa.Text))
                {
                    var menuReserva = this.Parent as TabbedPage;
                    menuReserva.CurrentPage = menuReserva.Children[2];
                    menuReserva.Children.RemoveAt(0);
                }            
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "Ok");
            }
        }

        public void ComponentesAtivarMesa(bool visible)
        {
            lblInformacaoAtivar.IsVisible = visible;
            txtNmrMesa.IsVisible = visible;
            txtSenhaMesa.IsVisible = visible;
            btnAtivar.IsVisible = visible;
        }

        public void ComponentesTempo(bool visible)
        {
            lblInformacaoTempo.IsVisible = visible;
            lblTempoLiberarMesa.IsVisible = visible;
        }


        private void CancelarReserva_Clicked(object sender, EventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            CancelarReserva();
            ((Button)sender).IsEnabled = true;
        }

        private void txt_Focused(object sender, FocusEventArgs e)
        {
            Aplicacao.MostrarLabel(true, (Entry)sender);
        }

        private void txt_Unfocused(object sender, FocusEventArgs e)
        {
            if (((Entry)sender).Text == string.Empty || ((Entry)sender).Text == null)
                Aplicacao.MostrarLabel(false, (Entry)sender);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CarregaDados();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            lvPedidosPendentes.ItemsSource = null;
        }

        private void ButtonAtivarReserva_Clicked(object sender, EventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            AtivarReserva();
            ((Button)sender).IsEnabled = true;
        }
    }
}