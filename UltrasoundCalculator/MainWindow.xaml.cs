using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace UltrasoundCalculator {
    public partial class MainWindow : Window {
        public MainWindow() => InitializeComponent();

        bool editing = false;
        readonly HashSet<object> edited = new HashSet<object>();
        object firstSender;

        void StartEditing(object sender) {
            if (!editing) {
                editing = true;
                firstSender = sender;
            }
            if (!edited.Contains(sender))
                edited.Add(sender);
        }

        void EndEditing(object sender) {
            if (firstSender == sender) {
                editing = false;
                edited.Clear();
            }
        }

        bool Parse(TextBox from, out double value) {
            if (from != null)
                return double.TryParse(from.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            else {
                value = double.NaN;
                return false;
            }
        }

        bool ParseDivisor(TextBox from, out double value) {
            bool result = Parse(from, out value);
            return result && value != 0;
        }

        void Apply(TextBox to, double value) {
            if (to != null && !edited.Contains(to)) {
                to.Text = value.ToString("0.000000").TrimEnd('0').TrimEnd(',', '.');
                edited.Add(to);
            }
        }

        void SolveDamping(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(b, out double _b) && Parse(f, out double _f))
                Apply(beta, _b * _f);
            EndEditing(sender);
        }

        void SolveDampingBeta(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (ParseDivisor(beta, out double _beta)) {
                if (ParseDivisor(b, out double _b))
                    Apply(f, _beta / _b);
                else if (ParseDivisor(f, out double _f))
                    Apply(b, _beta / _f);
                if (Parse(rx_db, out double _rx_db))
                    Apply(d, Math.Abs(_rx_db / _beta));
            }
            EndEditing(sender);
        }

        void SolvePenetrationIDiv(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(ix, out double _ix) && ParseDivisor(i0, out double _i0))
                Apply(ipc, _ix / _i0 * 100);
            EndEditing(sender);
        }

        void SolvePenetrationIPercent(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (ParseDivisor(ipc, out double _ipc)) {
                if (Parse(i0, out double _i0)) {
                    Apply(ix, _ipc / 100 * _i0);
                    if (Parse(ix, out double _ix))
                        Apply(rx, _ix / _i0);
                } else if (ParseDivisor(ix, out double _ix)) {
                    Apply(i0, _ix / _ipc * 100);
                    if (Parse(i0, out double _i0_2))
                        Apply(rx, _ix / _i0_2);
                } else {
                    Apply(ix, 1);
                    Apply(i0, 100 / _ipc);
                    if (Parse(i0, out double _i0_2))
                        Apply(rx, 1 / _i0_2);
                }
            }
            EndEditing(sender);
        }

        void SolvePenetrationRxRatio(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(rx, out double _rx)) {
                Apply(rx_db, 10 * Math.Log10(_rx));
                Apply(ipc, 100 * _rx);
                Apply(pdiv, Math.Sqrt(_rx));
            }
            EndEditing(sender);
        }

        void SolvePenetrationPerfRatio(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(pdiv, out double _pdiv))
                Apply(rx, _pdiv * _pdiv);
            EndEditing(sender);
        }

        void SolvePenetrationRxDecibel(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(rx_db, out double _rx_db)) {
                Apply(rx, Math.Pow(10, _rx_db * .1));
                if (ParseDivisor(beta, out double _beta))
                    Apply(d, Math.Abs(_rx_db / _beta));
                if (Parse(r_db, out double _r_db))
                    Apply(til_db, _r_db + _rx_db);
            }
            EndEditing(sender);
        }

        void SolvePenetrationDistance(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(d, out double _d) && Parse(beta, out double _beta))
                Apply(rx_db, - Math.Abs(_d * _beta));
            EndEditing(sender);
        }

        void SolveEchoImpedance(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(za1, out double _za1) && Parse(za2, out double _za2)) {
                double res = Math.Pow((_za2 - _za1) / (_za2 + _za1), 2);
                Apply(r, res);
                Apply(r_db, 10 * Math.Log10(res));
            }
            EndEditing(sender);
        }

        void SolveEchoR(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(r, out double _r) && Parse(za2, out double _za2)) {
                double sqr = Math.Sqrt(_r);
                double mp = (1 - sqr) * _za2 / (1 + sqr);
                double pm = (1 + sqr) * _za2 / (1 - sqr);
                Apply(za1, mp);
                if (Parse(za1, out double _za1))
                    za1other.Content = (Math.Abs(mp - _za1) < Math.Abs(pm - _za1) ? pm : mp).ToString("or 0.000000");
                Apply(r_db, 10 * Math.Log10(_r));
            }
            EndEditing(sender);
        }

        void SolveEchoRDecibel(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(r_db, out double _r_db)) {
                Apply(r, Math.Pow(10, _r_db * .1));
                if (Parse(rx_db, out double _rx_db))
                    Apply(til_db, _r_db + _rx_db);
            }
            EndEditing(sender);
        }

        void SolveTotalIntensityLossDecibel(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(til_db, out double _til_db)) {
                if (Parse(rx_db, out double _rx_db))
                    Apply(r_db, _til_db - _rx_db);
                Apply(til, Math.Pow(10, _til_db * .1));
            }
            EndEditing(sender);
        }

        void SolveTotalIntensityLoss(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(til, out double _til)) {
                Apply(til_db, 10 * Math.Log10(_til));
                Apply(tpl, Math.Sqrt(_til));
            }
            EndEditing(sender);
        }

        void SolveTotalPerformanceLoss(object sender, TextChangedEventArgs e) {
            StartEditing(sender);
            if (Parse(tpl, out double _tpl))
                Apply(til, _tpl * _tpl);
            EndEditing(sender);
        }

        void FieldFocus(object sender, RoutedEventArgs e) {
            TextBox field = (TextBox)e.OriginalSource;
            field.Dispatcher.BeginInvoke(new Action(delegate {
                field.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
    }
}