using AutoMapper;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Contracts;
using DormitoryManagementSystem.DTO.SearchCriteria; // Criteria
using DormitoryManagementSystem.Entity;
using DormitoryManagementSystem.Utils;

namespace DormitoryManagementSystem.BUS.Implementations
{
    public class ContractBUS : IContractBUS
    {
        private readonly IContractDAO _contractDAO;
        private readonly IStudentDAO _studentDAO;
        private readonly IRoomDAO _roomDAO;
        private readonly IMapper _mapper;

        public ContractBUS(IContractDAO contractDAO, IStudentDAO studentDAO, IRoomDAO roomDAO, IMapper mapper)
        {
            _contractDAO = contractDAO;
            _studentDAO = studentDAO;
            _roomDAO = roomDAO;
            _mapper = mapper;
        }

        // ======================== GET METHODS ========================

        public async Task<IEnumerable<ContractReadDTO>> GetAllContractsAsync()
        {
            var list = await _contractDAO.SearchContractsAsync(new ContractSearchCriteria());
            return _mapper.Map<IEnumerable<ContractReadDTO>>(list);
        }

        public async Task<ContractReadDTO?> GetContractByIDAsync(string id)
        {
            var contract = await _contractDAO.GetContractByIDAsync(id);
            return contract == null ? null : _mapper.Map<ContractReadDTO>(contract);
        }

        public async Task<IEnumerable<ContractReadDTO>> GetContractsByStudentIDAsync(string studentId)
        {
            var list = await _contractDAO.SearchContractsAsync(new ContractSearchCriteria { StudentID = studentId });
            return _mapper.Map<IEnumerable<ContractReadDTO>>(list);
        }

        public async Task<IEnumerable<ContractReadDTO>> GetContractsAsync(string searchTerm)
        {
            var list = await _contractDAO.SearchContractsAsync(new ContractSearchCriteria { SearchTerm = searchTerm });
            return _mapper.Map<IEnumerable<ContractReadDTO>>(list);
        }

        public async Task<IEnumerable<ContractReadDTO>> GetContractsByMultiConditionAsync(ContractFilterDTO filter)
        {
            // Logic swap ngày nếu bị ngược
            if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate > filter.ToDate)
                (filter.FromDate, filter.ToDate) = (filter.ToDate, filter.FromDate);

            var criteria = new ContractSearchCriteria
            {
                BuildingID = filter.BuildingID,
                Status = filter.Status,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate
            };

            var list = await _contractDAO.SearchContractsAsync(criteria);
            return _mapper.Map<IEnumerable<ContractReadDTO>>(list);
        }

        public async Task<ContractDetailDTO?> GetContractFullDetailAsync(string studentId)
        {
            var contract = await _contractDAO.GetContractDetailAsync(studentId);
            if (contract == null) return null;

            return new ContractDetailDTO
            {
                ContractID = contract.Contractid,
                StartTime = contract.Starttime.ToDateTime(TimeOnly.MinValue),
                EndTime = contract.Endtime.ToDateTime(TimeOnly.MinValue),
                Status = contract.Status ?? "N/A",
                StudentID = contract.Studentid,
                StudentName = contract.Student?.Fullname ?? "N/A",
                Gender = contract.Student?.Gender ?? "N/A",
                Major = contract.Student?.Major ?? "N/A",
                PhoneNumber = contract.Student?.Phonenumber ?? "N/A",
                Email = contract.Student?.Email ?? "N/A",
                CCCD = contract.Student?.Idcard ?? "N/A",
                Address = contract.Student?.Address ?? "N/A",
                RoomID = contract.Roomid,
                RoomNumber = contract.Room?.Roomnumber ?? 0,
                BuildingName = contract.Room?.Building?.Buildingname ?? "Unknown"
            };
        }

        // ======================== TRANSACTION METHODS ========================

        public async Task<string> RegisterContractAsync(string studentId, ContractRegisterDTO dto)
        {
            if (dto.EndTime <= dto.StartTime)
                throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");

            var activeContract = await _contractDAO.GetActiveContractByStudentIDAsync(studentId);
            if (activeContract != null)
                throw new InvalidOperationException($"Bạn hiện đang có hợp đồng tại phòng {activeContract.Roomid}.");

            var room = await _roomDAO.GetRoomByIDAsync(dto.RoomID)
                       ?? throw new KeyNotFoundException("Phòng không tồn tại.");

            if (room.Status != AppConstants.RoomStatus.Active)
                throw new InvalidOperationException("Phòng này đang bảo trì/không hoạt động.");

            if (room.Currentoccupancy >= room.Capacity)
                throw new InvalidOperationException("Phòng này đã đầy.");

            var newContract = new Contract
            {
                Contractid = $"CTR_{DateTime.Now:yyyyMMdd}_{new Random().Next(1000, 9999)}",
                Studentid = studentId,
                Roomid = dto.RoomID,
                Starttime = DateOnly.FromDateTime(dto.StartTime),
                Endtime = DateOnly.FromDateTime(dto.EndTime),
                Status = AppConstants.ContractStatus.Pending, // Chờ duyệt
                Createddate = DateTime.Now
            };

            await _contractDAO.AddContractAsync(newContract);
            return newContract.Contractid;
        }

        public async Task<string> AddContractAsync(ContractCreateDTO dto, string staffUserID)
        {
            if (await _contractDAO.GetContractByIDAsync(dto.ContractID) != null)
                throw new InvalidOperationException($"Hợp đồng {dto.ContractID} đã tồn tại.");

            if (dto.EndTime <= dto.StartTime)
                throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");

            // Kiểm tra sinh viên
            if (await _studentDAO.GetStudentByIDAsync(dto.StudentID) == null)
                throw new KeyNotFoundException($"Không tìm thấy sinh viên {dto.StudentID}.");

            // Kiểm tra trùng hợp đồng
            var activeContract = await _contractDAO.GetActiveContractByStudentIDAsync(dto.StudentID);
            if (activeContract != null)
                throw new InvalidOperationException($"Sinh viên này đã có hợp đồng tại phòng {activeContract.Roomid}.");

            // Kiểm tra phòng
            var room = await _roomDAO.GetRoomByIDAsync(dto.RoomID)
                       ?? throw new KeyNotFoundException($"Không tìm thấy phòng {dto.RoomID}.");

            if (room.Status != AppConstants.RoomStatus.Active)
                throw new InvalidOperationException($"Phòng {dto.RoomID} không hoạt động.");

            if (room.Currentoccupancy >= room.Capacity)
                throw new InvalidOperationException($"Phòng {dto.RoomID} đã đầy.");

            var contract = _mapper.Map<Contract>(dto);
            contract.Createddate = DateTime.Now;
            contract.Staffuserid = staffUserID;

            await _contractDAO.AddContractAsync(contract);

            // Nếu tạo hợp đồng Active ngay thì tăng slot phòng
            if (contract.Status == AppConstants.ContractStatus.Active)
            {
                room.Currentoccupancy += 1;
                await _roomDAO.UpdateRoomAsync(room);
            }
            return contract.Contractid;
        }

        public async Task UpdateContractAsync(string id, ContractUpdateDTO dto)
        {
            var contract = await _contractDAO.GetContractByIDAsync(id)
                           ?? throw new KeyNotFoundException($"Hợp đồng {id} không tồn tại.");

            if (dto.EndTime <= dto.StartTime) throw new ArgumentException("Thời gian không hợp lệ.");

            // Logic phức tạp: Đổi phòng hoặc đổi trạng thái
            bool isRoomChanged = contract.Roomid != dto.RoomID;
            bool isStatusChanged = (contract.Status ?? AppConstants.ContractStatus.Active) != dto.Status;

            if (isRoomChanged)
            {
                // Trả slot phòng cũ
                if (contract.Status == AppConstants.ContractStatus.Active)
                {
                    var oldRoom = await _roomDAO.GetRoomByIDAsync(contract.Roomid);
                    if (oldRoom != null && oldRoom.Currentoccupancy > 0)
                    {
                        oldRoom.Currentoccupancy--;
                        await _roomDAO.UpdateRoomAsync(oldRoom);
                    }
                }
                // Chiếm slot phòng mới
                if (dto.Status == AppConstants.ContractStatus.Active)
                {
                    var newRoom = await _roomDAO.GetRoomByIDAsync(dto.RoomID)
                                  ?? throw new KeyNotFoundException($"Phòng mới {dto.RoomID} không tồn tại.");

                    if (newRoom.Currentoccupancy >= newRoom.Capacity)
                        throw new InvalidOperationException($"Phòng {dto.RoomID} đã đầy.");

                    newRoom.Currentoccupancy++;
                    await _roomDAO.UpdateRoomAsync(newRoom);
                }
            }
            else if (isStatusChanged)
            {
                var room = await _roomDAO.GetRoomByIDAsync(contract.Roomid);
                if (room != null)
                {
                    // Active -> Non-Active: Giảm slot
                    if (contract.Status == AppConstants.ContractStatus.Active && dto.Status != AppConstants.ContractStatus.Active)
                    {
                        if (room.Currentoccupancy > 0) room.Currentoccupancy--;
                    }
                    // Non-Active -> Active: Tăng slot
                    else if (contract.Status != AppConstants.ContractStatus.Active && dto.Status == AppConstants.ContractStatus.Active)
                    {
                        if (room.Currentoccupancy >= room.Capacity) throw new InvalidOperationException("Phòng đã đầy.");
                        room.Currentoccupancy++;
                    }
                    await _roomDAO.UpdateRoomAsync(room);
                }
            }

            _mapper.Map(dto, contract);
            contract.Contractid = id;
            await _contractDAO.UpdateContractAsync(contract);
        }

        public async Task DeleteContractAsync(string id)
        {
            var contract = await _contractDAO.GetContractByIDAsync(id)
                           ?? throw new KeyNotFoundException($"Hợp đồng {id} không tồn tại.");

            // Nếu xóa HĐ đang Active, trả lại slot phòng
            if (contract.Status == AppConstants.ContractStatus.Active)
            {
                var room = await _roomDAO.GetRoomByIDAsync(contract.Roomid);
                if (room != null && room.Currentoccupancy > 0)
                {
                    room.Currentoccupancy--;
                    await _roomDAO.UpdateRoomAsync(room);
                }
            }
            await _contractDAO.DeleteContractAsync(id);
        }
    }
}