using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data.Common.PaginationModel;
using Data.DataAccess;
using Data.Entities;
using Data.Enum;
using Data.Models;
using Data.Utils.Paging;
using MongoDB.Driver;
using Services.SignalR;
using static Confluent.Kafka.ConfigPropertyNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.Core
{
    public interface INotificationService
    {
        Task Add(Notification model);
        Task CreateBooking(KafkaModel model);
        Task CreateSearchRequest(KafkaModel model);
        Task ArrivedBooking(KafkaModel model);
        Task CheckInBooking(KafkaModel model);
        Task OnGoingBooking(KafkaModel model);
        Task CheckOutBooking(KafkaModel model);
        Task CompleteBooking(KafkaModel model);
        Task DriverCancel(KafkaModel model);
        Task CustomerCancel(KafkaModel model);
        Task ChangeDriver(KafkaModel model);
        Task OldDriver(KafkaModel model);
        Task SearchRequestCancel(KafkaModel model);
        Task WalletAddFundsSuccess(KafkaModel model);
        Task MoMoTransactionFail(KafkaModel model);
        Task VNPayTransactionFail(KafkaModel model);
        Task WalletWithDrawsFundsRequest(KafkaModel model);
        Task WalletWithRawsFundsSuccess(KafkaModel model);
        Task WalletWithRawsFundsFailure(KafkaModel model);
        Task PaymentBookingSuccess(KafkaModel model);
        //Task WalletRefundAdmin(KafkaModel model);
        Task WalletRefundCustomer(KafkaModel model);
        //Task WalletDriverIncomeAdmin(KafkaModel model);
        Task WalletIncomeDriver(KafkaModel model);
        Task SearchRequestDriverMiss(KafkaModel model);
        Task DriverStatusBan(KafkaModel model);
        Task DriverStatusWarning(KafkaModel model);
        Task WalletAddFundsBooking(KafkaModel model);
        Task EmergencyCustomerSend(KafkaModel model);
        Task EmergencyDriverSend(KafkaModel model);
        Task EmergencyStaffSolve(KafkaModel model);
        Task EmergencyStopTrip(KafkaModel model);
        Task<ResultModel> Get(PagingParam<SortMessageCriteria> paginationModel, Guid userId);
        Task<ResultModel> GetById(Guid Id);
        Task<ResultModel> SeenNotify(Guid id, Guid userId);
        Task<ResultModel> SeenAllNotify(Guid userId);
        Task<ResultModel> DeleteNotify(Guid id, Guid userId);
    }
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly INotificationHub _notificationHub;
        private readonly IFireBaseNotificationService _fireBaseNotificationService;
        public NotificationService(AppDbContext dbContext, IMapper mapper, INotificationHub notificationHub, IFireBaseNotificationService fireBaseNotificationService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _notificationHub = notificationHub;
            _fireBaseNotificationService = fireBaseNotificationService;
        }

        public async Task Add(Notification notification)
        {
            try
            {
                await _dbContext.Notifications.InsertOneAsync(notification);
                await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());

            }
            catch (Exception e)
            {
                var r = e;
            }
        }

        public async Task CreateSearchRequest(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Có một cuốc xe ở gần bạn!",
                        Body = "Hãy nhận cuốc xe ngay nào!",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "SearchRequest"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task CreateBooking(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Cuốc xe của bạn đã có tài xế!",
                        Body = "Tài xế đang di chuyển tới điểm đón. Vui lòng chờ tài xế đến nhé.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Booking"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task ArrivedBooking(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Tài xế đã đến!",
                        Body = "Tài xế đang ở điểm đón của bạn. Họ đang chờ bạn đấy!",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Booking"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }
        public async Task CheckInBooking(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Tài xế đang kiểm tra thông tin",
                        Body = "Tài xế đang xác thực tình trạng người đi và xe.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Booking"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task OnGoingBooking(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Chuyến xe bắt đầu di chuyển!",
                        Body = "Tài xế đã bắt đầu chuyến xe.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Booking"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task CheckOutBooking(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Tài xế đã tới điểm đến!",
                        Body = "Tài xế đang kiểm tra lại xe và người đi sau chuyến đi.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Booking"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task CompleteBooking(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Chuyến xe đã được hoàn thành!",
                        Body = "Tài xế đã hoàn thành chuyến đi của bạn.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Booking"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                    var driverOnlines = _dbContext.DriverOnlines.Find(_ => _.CustomerId == notification.UserId).FirstOrDefault();
                    if (driverOnlines != null)
                    {
                        await _dbContext.DriverOnlines.DeleteOneAsync(_ => _.Id == driverOnlines.Id);
                    }
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task DriverCancel(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Đơn của bạn đã bị hủy!",
                        Body = "Tài xế đã hủy cuốc xe của bạn.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Booking"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task CustomerCancel(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Khách hàng đã hủy cuốc xe",
                        Body = "Khách hàng đã hủy đơn đặt dịch vụ của họ.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Booking"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task ChangeDriver(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Có một cuốc xe ở gần bạn!",
                        Body = "Hãy nhận cuốc xe ngay nào!",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "SearchRequest"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task OldDriver(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "SearchRequest"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task SearchRequestCancel(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Cuốc xe đã bị hủy",
                        Body = "Khách hàng đã hủy đơn đặt dịch vụ của họ.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "SearchRequest"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task WalletAddFundsSuccess(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Biến động số dư Ví",
                        Body = "Ví của bạn đã được nạp tiền thành công.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "WalletAddFunds"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task MoMoTransactionFail(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Giao dịch thất bại",
                        Body = "Có lỗi xảy ra khi giao dịch với MoMo.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "WalletAddFunds"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task VNPayTransactionFail(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Giao dịch thất bại",
                        Body = "Có lỗi xảy ra khi giao dịch với VNPay.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "WalletAddFunds"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task WalletWithDrawsFundsRequest(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Có yêu cầu rút tiền mới",
                        Body = "Vui lòng duyệt yêu cầu của người dùng!",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "WalletWithDrawFunds"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task WalletWithRawsFundsSuccess(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Biến động số dư Ví",
                        Body = "Bạn đã rút tiền trong Ví thành công.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "WalletWithDrawFunds"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task WalletWithRawsFundsFailure(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Giao dịch thất bại",
                        Body = "Giao dịch rút tiền của bạn đã thất bại.Hãy thử lại sau.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "WalletWithDrawFunds"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task PaymentBookingSuccess(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Thanh toán thành công!",
                        Body = "Bạn đã thanh toán thành công, chúng tôi sẽ bắt đầu tìm tài xế cho bạn.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "PaymentBookingSuccess"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task WalletRefundCustomer(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Biến động số dư Ví",
                        Body = "Số tiền thanh toán cho chuyến đi đã được hoàn lại Ví.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "WalletRefund"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        //public async Task WalletRefundAdmin(KafkaModel kafkaModel)
        //{
        //    try
        //    {
        //        foreach (var item in kafkaModel.UserReceiveNotice)
        //        {
        //            var notification = new Notification
        //            {
        //                Title = "Biến động số dư Ví",
        //                Body = "Số tiền của khách đã được chuyển lại do hủy chuyến.",
        //                Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
        //                UserId = item,
        //                TypeModel = "WalletRefund"
        //            };
        //            SendNotifyFcm(item, notification, notification.Title, notification.Body);
        //            await _dbContext.Notifications.InsertOneAsync(notification);
        //            await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
        //    }
        //}

        //public async Task WalletDriverIncomeAdmin(KafkaModel kafkaModel)
        //{
        //    try
        //    {
        //        foreach (var item in kafkaModel.UserReceiveNotice)
        //        {
        //            var notification = new Notification
        //            {
        //                Title = "Biến động số dư Ví",
        //                Body = "Số tiền trả cho Driver khi hoàn thành chuyến",
        //                Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
        //                UserId = item,
        //                TypeModel = "WalletIncome"
        //            };
        //            SendNotifyFcm(item, notification, notification.Title, notification.Body);
        //            await _dbContext.Notifications.InsertOneAsync(notification);
        //            await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
        //    }
        //}

        public async Task WalletIncomeDriver(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Biến động số dư Ví",
                        Body = "Số tiền nhận được khi hoàn thành chuyến",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "WalletIncome"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task SearchRequestDriverMiss(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    await _notificationHub.SearchRequestDriverMiss(item.ToString(), (string)kafkaModel.Payload);
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task DriverStatusBan(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Thông báo tình trạng tài xế",
                        Body = "Tài khoản tài xế đã bị khóa vì đã hủy quá nhiều chuyến",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "DriverStatus"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task DriverStatusWarning(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Thông báo tình trạng tài xế",
                        Body = "Tài khoản tài xế bị cảnh cáo vì đã hủy quá nhiều chuyến",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "DriverStatus"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task WalletAddFundsBooking(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Biến động số dư Ví",
                        Body = "Ví của bạn đã được nạp tiền thành công.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "AddFundsBooking"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task EmergencyCustomerSend(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Sự cố khẩn cấp",
                        Body = "Khách hàng vừa gửi một sự cố khẩn cấp cần giải quyết.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Emergency"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task EmergencyDriverSend(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Sự cố khẩn cấp",
                        Body = "Tài xế vừa gửi một sự cố khẩn cấp cần giải quyết.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Emergency"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task EmergencyStaffSolve(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Sự cố đã được giải quyết",
                        Body = " Sự cố của bạn đã được lưu lại trên hệ thống.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Emergency"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task EmergencyStopTrip(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Xác nhận hủy chuyến xe",
                        Body = "Chuyến xe được xác nhận hủy bởi nhân viên.",
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Booking"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task<ResultModel> Get(PagingParam<SortMessageCriteria> paginationModel, Guid userId)
        {
            var result = new ResultModel();
            result.Succeed = false;
            try
            {
                var notifications = _dbContext.Notifications.Find(_ => _.UserId == userId && !_.IsDeleted)
                     .SortByDescending(_n => _n.DateCreated);

                var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, notifications.CountDocuments());
                var sortedData = notifications.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
                var pagedData = sortedData.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
                var viewModels = pagedData.ToEnumerable().AsQueryable().ProjectTo<NotificationModel>(_mapper.ConfigurationProvider);

                paging.Data = viewModels;
                result.Data = paging;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> GetById(Guid id)
        {
            var result = new ResultModel();
            result.Succeed = false;
            try
            {
                var notification = _dbContext.Notifications.Find(_ => _.Id == id && !_.IsDeleted).FirstOrDefault();
                var viewData = _mapper.Map<NotificationModel>(notification);
                //var dataDeserial = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(viewData.Data!.ToString()!);
                //viewData.Data = dataDeserial;
                result.Succeed = true;
                result.Data = viewData;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> SeenNotify(Guid id, Guid userId)
        {
            var result = new ResultModel();
            try
            {
                var notification = _dbContext.Notifications.Find(_ => _.Id == id && _.UserId == userId && !_.IsDeleted).FirstOrDefault();
                if (notification == null)
                {
                    result.Succeed = false;
                    result.ErrorMessage = "Notification not found";
                    return result;
                }
                var user = _dbContext.Users.Find(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
                if (user == null)
                {
                    result.Succeed = false;
                    result.ErrorMessage = "User not exist";
                    return result;
                }

                if (user.CurrenNoticeCount > 0)
                {
                    user.CurrenNoticeCount -= 1;
                    _dbContext.Users.ReplaceOne(_ => _.Id == user.Id, user);
                }
                notification.Seen = true;
                _dbContext.Notifications.ReplaceOne(_ => _.Id == notification.Id, notification);

                await _notificationHub.NewNotificationCount(user.CurrenNoticeCount, user.Id.ToString());

                result.Data = notification.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> SeenAllNotify(Guid userId)
        {
            var result = new ResultModel();
            try
            {
                var user = _dbContext.Users.Find(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
                if (user == null)
                {
                    result.Succeed = false;
                    result.ErrorMessage = "User not exist";
                    return result;
                }

                var notifications = _dbContext.Notifications.Find(_ => _.UserId == userId && !_.IsDeleted)
                     .ToList()
                     .OrderByDescending(_n => _n.DateCreated).ToList();
                if (notifications == null)
                {
                    result.Succeed = false;
                    result.ErrorMessage = "Notification not found";
                    return result;
                }
                foreach (var item in notifications)
                {
                    item.Seen = true;
                    _dbContext.Notifications.ReplaceOne(_ => _.Id == item.Id, item);
                }
                user.CurrenNoticeCount = 0;
                _dbContext.Users.ReplaceOne(_ => _.Id == user.Id, user);

                await _notificationHub.NewNotificationCount(user.CurrenNoticeCount, user.Id.ToString());

                result.Data = _mapper.Map<List<NotificationModel>>(notifications);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> DeleteNotify(Guid id, Guid userId)
        {
            var result = new ResultModel();
            try
            {
                var notification = _dbContext.Notifications.Find(_ => _.Id == id && _.UserId == userId && !_.IsDeleted).FirstOrDefault();
                if (notification == null)
                {
                    result.Succeed = false;
                    result.ErrorMessage = "Notification not found";
                    return result;
                }
                _dbContext.Notifications.DeleteOne(_ => _.Id == notification.Id);
                result.Data = notification.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        private async void SendNotifyFcm(Guid userReceiveId, Notification data, string title, string body)
        {
            try
            {
                var userReceive = _dbContext.Users.Find(_ => _.Id == userReceiveId && !_.IsDeleted).FirstOrDefault();
                if (userReceive != null)
                {
                    userReceive.CurrenNoticeCount++;
                    await _notificationHub.NewNotificationCount(userReceive.CurrenNoticeCount, userReceive.Id.ToString());
                    _dbContext.Users.ReplaceOne(_ => _.Id == userReceiveId, userReceive);
                    if (userReceive != null && userReceive.FcmTokens.Count > 0)
                    {
                        var result = new NotificationFcmModel
                        {
                            Title = title,
                            Body = body,
                            Data = data,
                            RegistrationIds = userReceive.FcmTokens,
                            UserId = userReceiveId,
                        };
                        await _fireBaseNotificationService.SendNotification(result);
                    }
                }

            }
            catch (Exception ex)
            {

            }

        }

    }


}

