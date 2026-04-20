namespace TaskManagement.Models;

public enum Role { User, Admin }

public class User : BaseEntity
{



    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public Role Role { get; set; } = Role.User;

}



// Rất tốt, mình thấy bạn đã có một lớp User với các thuộc tính cơ bản: Username, PasswordHash, và Role. Bây giờ, để mình chia nhỏ bài tập cho bạn:

// Viết một class mới TaskItem, mô tả các thuộc tính như: tiêu đề, mô tả, trạng thái, và người được giao (có thể liên kết tới User).
// Thực hiện quan hệ giữa TaskItem và User (giống như bạn đã làm trong AppDbContext), đảm bảo mỗi task có thể thuộc về một user.
// Viết một bài tập nhỏ: tạo một TaskItem, gán nó cho một User, rồi lưu vào cơ sở dữ liệu.

// Sau khi bạn viết xong từng bước, bạn gửi lại cho mình. Mình sẽ kiểm tra, chấm điểm và hướng dẫn bạn từng chút một, để bạn nắm vững.


public enum Status { Todo, progress, done }
public class TaskItems
{

    public int Title { get; set; }
    public string description { get; set; }
    public Status Status { get; set; }
    public Role Role { get; set; }
}