using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Accel
{
    public partial class MainPage : ContentPage
    {
        private bool isFlashOn = false;
        private bool isRotatingRight = false;

        public MainPage()
        {
            InitializeComponent();
        }

        //Verifica si el acelerómetro ya está siendo monitoreado y, si no lo está, lo inicia con la velocidad predeterminada
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Verificar si el acelerómetro ya está siendo monitoreado
            if (Accelerometer.IsMonitoring)
                return;

            // Iniciar el monitoreo del acelerómetro con la velocidad predeterminada
            Accelerometer.Start(SensorSpeed.Default);

            // Suscribirse al evento ReadingChanged para recibir notificaciones de cambios en los valores del acelerómetro
            Accelerometer.ReadingChanged += Acelerometro_ReadingChanged;
        }

        //sDetiene el monitoreo del acelerómetro llamando a Accelerometer.Stop() y se desuscribe del evento ReadingChanged
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Detener el monitoreo del acelerómetro
            Accelerometer.Stop();

            // Desuscribirse del evento ReadingChanged para dejar de recibir notificaciones de cambios en los valores del acelerómetro
            Accelerometer.ReadingChanged -= Acelerometro_ReadingChanged;
        }

        //se ejecuta cada vez que se produce un cambio en los valores del acelerómetro
        void Acelerometro_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            // Obtener los datos del acelerómetro del evento
            var data = e.Reading;

            // Extraer los valores de aceleración en los ejes X, Y y Z
            var x = data.Acceleration.X;
            var y = data.Acceleration.Y;
            var z = data.Acceleration.Z;

            // Actualizar la interfaz de usuario en el subproceso principal
            Device.BeginInvokeOnMainThread(() =>
            {
                // Asignar la cadena formateada al Label para mostrar los valores del acelerómetro
                accelerometerLabel.Text = $"X: {x:F2}\nY: {y:F2}\nZ: {z:F2}";
                const double threshold = 0.60;

                if (x > threshold)
                {
                    isRotatingRight = true;

                    if (!isFlashOn)
                    {
                        TurnOnFlash();
                    }
                }
                else
                {
                    isRotatingRight = false;
                }
            });
        }
        
        async void TurnOnFlash()
        {
            try
            {
                await Flashlight.TurnOnAsync();
                isFlashOn = true;
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Error", "La linterna no está soportada en tu dispositivo", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al encender la linterna: {ex.Message}", "OK");
            }
        }

        async void TurnOffFlash()
        {
            try
            {
                await Flashlight.TurnOffAsync();
                isFlashOn = false;
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Error", "La linterna no está soportada en tu dispositivo", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al apagar la linterna: {ex.Message}", "OK");
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!isRotatingRight && isFlashOn)
            {
                TurnOffFlash();
            }
        }
    }
}
