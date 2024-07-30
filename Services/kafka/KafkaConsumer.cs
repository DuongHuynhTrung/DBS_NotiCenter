using Confluent.Kafka;
using Data.Models;
using Services.Core;

namespace Services.kafka
{
    public class KafkaConsumer : BackgroundService
    {
        private ConsumerConfig _config;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly ILocationService _locationService;

        public KafkaConsumer(ConsumerConfig config, IUserService userService, INotificationService notificationService, ILocationService locationService)
        {
            _config = config;
            _userService = userService;
            _notificationService = notificationService;
            _locationService = locationService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => Start(stoppingToken));
            return Task.CompletedTask;
        }

        private void Start(CancellationToken stoppingToken)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
            using (var c = new ConsumerBuilder<Ignore, string>(_config).Build())
            {
                var topics = new List<string>() {
                    "dbs-user-create-new",
                    "dbs-user-update",
                    "dbs-search-request-create",
                    "dbs-booking-create",
                    "dbs-booking-status-accept",
                    "dbs-booking-status-arrived",
                    "dbs-booking-status-checkin",
                    "dbs-booking-status-ongoing",
                    "dbs-booking-status-checkout",
                    "dbs-booking-status-complete",
                    "dbs-booking-driver-cancel",
                    "dbs-booking-customer-cancel",
                    "dbs-booking-new-driver",
                    "dbs-booking-old-driver",
                    "dbs-searchrequest-customer-cancel",
                    "dbs-wallet-addfunds-success",
                    //"dbs-momo-transaction-fail",
                    //"dbs-vnpay-transaction-fail",
                    "dbs-wallet-withrawsfunds-request",
                    "dbs-wallet-withrawsfunds-success",
                    "dbs-wallet-withrawsfunds-failure",
                    "dbs-payment-booking-success",
                    //"dbs-wallet-refund-admin",
                    "dbs-wallet-refund-customer",
                    "dbs-wallet-driverincome-admin",
                    "dbs-wallet-income-driver",
                    "dbs-tracking-driver-location",
                    "dbs-get-driver-online",
                    "dbs-driver-status-online",
                    "dbs-driver-status-offline",
                    "dbs-driver-status-busy",
                    "dbs-driver-status-free",
                    "dbs-searchrequest-driver-miss",
                    "dbs-driver-status-ban",
                    "dbs-driver-status-warning",
                    "dbs-wallet-addfunds-booking",
                    "dbs-emergency-customer-send",
                    "dbs-emergency-driver-send",
                    "dbs-emergency-staff-solve",
                    "dbs-emergency-stop-trip"
                };
                c.Subscribe(topics);
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = c.Consume(cts.Token);
                    var data = "";
                    switch (cr.Topic)
                    {
                        case "dbs-user-create-new":
                            var userCreate = Newtonsoft.Json.JsonConvert.DeserializeObject<UserFromKafka>(cr.Value);
                            _userService.AddFromKafka(userCreate);
                            break;
                        case "dbs-user-update":
                            var userUpdate = Newtonsoft.Json.JsonConvert.DeserializeObject<UserFromKafka>(cr.Value);
                            _userService.UpdateFromKafka(userUpdate);
                            break;
                        case "dbs-search-request-create":
                            var createSearchRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.CreateSearchRequest(createSearchRequest);
                            break;
                        case "dbs-booking-create":
                            var createBooking = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.CreateBooking(createBooking);
                            break;
                        case "dbs-booking-status-arrived":
                            var arrivedBooking = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.ArrivedBooking(arrivedBooking);
                            break;
                        case "dbs-booking-status-checkin":
                            var checkInBooking = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.CheckInBooking(checkInBooking);
                            break;
                        case "dbs-booking-status-ongoing":
                            var ongoingBooking = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.OnGoingBooking(ongoingBooking);
                            break;
                        case "dbs-booking-status-checkout":
                            var checkOutBooking = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.CheckOutBooking(checkOutBooking);
                            break;
                        case "dbs-booking-status-complete":
                            var completeBooking = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.CompleteBooking(completeBooking);
                            break;
                        case "dbs-booking-driver-cancel":
                            var driverCancel = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.DriverCancel(driverCancel);
                            break;
                        case "dbs-booking-customer-cancel":
                            var customerCancel = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.CustomerCancel(customerCancel);
                            break;
                        case "dbs-booking-new-driver":
                            var changerDriver = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.ChangeDriver(changerDriver);
                            break;
                        case "dbs-booking-old-driver":
                            var oldDriver = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.OldDriver(oldDriver);
                            break;
                        case "dbs-searchrequest-customer-cancel":
                            var searchRequestCancel = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.SearchRequestCancel(searchRequestCancel);
                            break;
                        case "dbs-wallet-addfunds-success":
                            var walletAddFundsSuccess = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.WalletAddFundsSuccess(walletAddFundsSuccess);
                            break;
                        case "dbs-momo-transaction-fail":
                            var momoTransactionFail = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.MoMoTransactionFail(momoTransactionFail);
                            break;
                        case "dbs-vnpay-transaction-fail":
                            var vnpayTransactionFail = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.VNPayTransactionFail(vnpayTransactionFail);
                            break;
                        case "dbs-wallet-withrawsfunds-request":
                            var walletWithDrawsFundsRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.WalletWithDrawsFundsRequest(walletWithDrawsFundsRequest);
                            break;
                        case "dbs-wallet-withrawsfunds-success":
                            var walletWithRawsFundsSuccess = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.WalletWithRawsFundsSuccess(walletWithRawsFundsSuccess);
                            break;
                        case "dbs-wallet-withrawsfunds-failure":
                            var walletWithRawsFundsFailure = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.WalletWithRawsFundsFailure(walletWithRawsFundsFailure);
                            break;
                        case "dbs-payment-booking-success":
                            var paymentBookingSuccess = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.PaymentBookingSuccess(paymentBookingSuccess);
                            break;
                        //case "dbs-wallet-refund-admin":
                        //    var walletRefundAdmin = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                        //    _notificationService.WalletRefundAdmin(walletRefundAdmin);
                        //    break;
                        case "dbs-wallet-refund-customer":
                            var walletRefundCustomer = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.WalletRefundCustomer(walletRefundCustomer);
                            break;
                        //case "dbs-wallet-driverincome-admin":
                        //    var walletDriverIncomeAdmin = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                        //    _notificationService.WalletDriverIncomeAdmin(walletDriverIncomeAdmin);
                        //    break;
                        case "dbs-wallet-income-driver":
                            var walletIncomeDriver = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.WalletIncomeDriver(walletIncomeDriver);
                            break;
                        case "dbs-tracking-driver-location":
                            var trackingDriverLocation = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _locationService.TrackingDriverLocation(trackingDriverLocation);
                            break;
                        case "dbs-get-driver-online":
                            var getDriverOnline = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _locationService.GetDriverOnline(getDriverOnline);
                            break;
                        case "dbs-driver-status-online":
                            var driverStatusOnline = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _locationService.DriverStatusOnline(driverStatusOnline);
                            break;
                        case "dbs-driver-status-offline":
                            var driverStatusOffline = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _locationService.DriverStatusOffline(driverStatusOffline);
                            break;
                        case "dbs-driver-status-busy":
                            var driverStatusBusy = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _locationService.DriverStatusBusy(driverStatusBusy);
                            break;
                        case "dbs-driver-status-free":
                            var driverStatusFree = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _locationService.DriverStatusFree(driverStatusFree);
                            break;
                        case "dbs-searchrequest-driver-miss":
                            var searchRequestDriverMiss = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.SearchRequestDriverMiss(searchRequestDriverMiss);
                            break;
                        case "dbs-driver-status-ban":
                            var driverStatusBan = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.DriverStatusBan(driverStatusBan);
                            break;
                        case "dbs-driver-status-warning":
                            var driverStatusWarning = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.DriverStatusWarning(driverStatusWarning);
                            break;
                        case "dbs-wallet-addfunds-booking":
                            var walletAddFundsBooking = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.WalletAddFundsBooking(walletAddFundsBooking);
                            break;
                        case "dbs-emergency-customer-send":
                            var emergencyCustomerSend = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.EmergencyCustomerSend(emergencyCustomerSend);
                            break;
                        case "dbs-emergency-driver-send":
                            var emergencyDriverSend = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.EmergencyDriverSend(emergencyDriverSend);
                            break;
                        case "dbs-emergency-staff-solve":
                            var emergencyStaffSolve = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.EmergencyStaffSolve(emergencyStaffSolve);
                            break;
                        case "dbs-emergency-stop-trip":
                            var emergencyStopTrip = Newtonsoft.Json.JsonConvert.DeserializeObject<KafkaModel>(cr.Value);
                            _notificationService.EmergencyStopTrip(emergencyStopTrip);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

}

