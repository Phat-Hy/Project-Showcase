import React, { useState, useEffect } from 'react';
import { 
  LogOut, 
  User as UserIcon, 
  Layers, 
  Search, 
  UploadCloud, 
  CheckSquare, 
  Plus, 
  FileSpreadsheet, 
  AlertTriangle, 
  FileText, 
  Mail, 
  Link as LinkIcon, 
  Users,
  CheckCircle,
  XCircle,
  Trash2
} from 'lucide-react';

// --- TS Types matching C# Entities ---
interface Milestone {
  id: string;
  projectId: string;
  title: string;
  description: string;
  done: boolean;
  dateCompleted?: string;
  createdAt: string;
}

interface Project {
  id: string;
  name: string;
  pitch: string;
  description: string;
  demoUrl?: string;
  status: 'Draft' | 'Active' | 'At-Risk' | 'Suspended';
  storageUsedBytes: number;
  lastUpdatedAt: string;
  milestones: Milestone[];
  teamMembers: User[];
}

interface Job {
  id: string;
  projectId: string;
  project?: Project;
  title: string;
  category: 'Engineering' | 'Business' | 'Design' | 'Marketing';
  description: string;
  requirements: string;
  status: 'Open' | 'Closed';
  createdAt: string;
}

interface User {
  id: string;
  email: string;
  name: string;
  role: 'Student' | 'Founder' | 'Manager' | 'Guest';
  studentId?: string;
  contactLink?: string;
  cvUrl?: string;
  projectId?: string;
  project?: Project;
}

interface Application {
  id: string;
  application_status: 'Pending' | 'Approved' | 'Rejected';
  createdAt: string;
  job_title: string;
  project_name: string;
}

interface Candidate {
  id: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  createdAt: string;
  job_title: string;
  student_name: string;
  student_id?: string;
  student_email: string;
  student_contact?: string;
  student_cv?: string;
}

const renderInlineLinks = (text: string) => {
  const linkRegex = /\[([^\]]+)\]\(([^)]+)\)/g;
  const parts = [];
  let lastIndex = 0;
  let match;

  while ((match = linkRegex.exec(text)) !== null) {
    if (match.index > lastIndex) {
      parts.push(text.substring(lastIndex, match.index));
    }
    parts.push(
      <a 
        key={match.index} 
        href={match[2]} 
        target="_blank" 
        rel="noopener noreferrer" 
        className="text-cyan-400 hover:underline font-semibold"
      >
        {match[1]}
      </a>
    );
    lastIndex = linkRegex.lastIndex;
  }

  if (lastIndex < text.length) {
    parts.push(text.substring(lastIndex));
  }

  return parts.length > 0 ? parts : text;
};

const renderMarkdown = (text: string) => {
  if (!text) return <p className="text-slate-500 italic">Chưa có mô tả chi tiết.</p>;
  
  const lines = text.split('\n');
  return (
    <div className="space-y-2 text-slate-300 text-sm leading-relaxed">
      {lines.map((line, idx) => {
        const trimmed = line.trim();
        if (trimmed.startsWith('# ')) {
          return <h3 key={idx} className="text-lg font-heading font-bold text-slate-200 mt-4 mb-2">{trimmed.slice(2)}</h3>;
        }
        if (trimmed.startsWith('## ')) {
          return <h4 key={idx} className="text-base font-heading font-bold text-slate-200 mt-3 mb-1">{trimmed.slice(3)}</h4>;
        }
        if (trimmed.startsWith('- ') || trimmed.startsWith('* ')) {
          return <li key={idx} className="ml-4 list-disc text-slate-300">{renderInlineLinks(trimmed.slice(2))}</li>;
        }
        if (trimmed === '') {
          return <div key={idx} className="h-2"></div>;
        }
        return <p key={idx}>{renderInlineLinks(line)}</p>;
      })}
    </div>
  );
};

export default function App() {
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  
  // Dashboard Tabs
  const [activeTab, setActiveTab] = useState<string>('');

  // Local state feeds
  const [projects, setProjects] = useState<Project[]>([]);
  const [jobs, setJobs] = useState<Job[]>([]);
  const [studentApps, setStudentApps] = useState<Application[]>([]);
  const [candidates, setCandidates] = useState<Candidate[]>([]);
  const [searchQuery, setSearchQuery] = useState('');

  // Selected project modal
  const [selectedProject, setSelectedProject] = useState<Project | null>(null);

  // Student Profile forms
  const [contactLink, setContactLink] = useState('');
  const [cvUrl, setCvUrl] = useState('');
  const [profileMsg, setProfileMsg] = useState({ text: '', type: '' });

  // Founder Project details editor
  const [founderProject, setFounderProject] = useState<Project | null>(null);
  const [projectPitch, setProjectPitch] = useState('');
  const [projectDescription, setProjectDescription] = useState('');
  const [projectDemoUrl, setProjectDemoUrl] = useState('');
  const [uploadMsg, setUploadMsg] = useState({ text: '', type: '' });
  const [lastUploadedUrl, setLastUploadedUrl] = useState('');
  const [uploadProgress, setUploadProgress] = useState(false);

  // Milestone/Job creators
  const [newMilestoneTitle, setNewMilestoneTitle] = useState('');
  const [newMilestoneDesc, setNewMilestoneDesc] = useState('');
  const [newJobTitle, setNewJobTitle] = useState('');
  const [newJobCategory, setNewJobCategory] = useState<'Engineering' | 'Business' | 'Design' | 'Marketing'>('Engineering');
  const [newJobDesc, setNewJobDesc] = useState('');
  const [newJobReqs, setNewJobReqs] = useState('');

  // General Notification Banner
  const [notification, setNotification] = useState<{ text: string; type: 'success' | 'error' | '' }>({ text: '', type: '' });

  // Login form states
  const [loginEmail, setLoginEmail] = useState('');
  const [loginPassword, setLoginPassword] = useState('');

  // Register form states
  const [isRegister, setIsRegister] = useState(false);
  const [regEmail, setRegEmail] = useState('');
  const [regPassword, setRegPassword] = useState('');
  const [regName, setRegName] = useState('');
  const [regRole, setRegRole] = useState<'Student' | 'Founder'>('Student');
  const [regStudentId, setRegStudentId] = useState('');
  const [regProjectName, setRegProjectName] = useState('');
  const [regProjectPitch, setRegProjectPitch] = useState('');

  useEffect(() => {
    fetchSession();
  }, []);

  useEffect(() => {
    if (currentUser && !activeTab) {
      // Set default tab based on role
      if (currentUser.role === 'Student') {
        setActiveTab('projects');
        fetchStudentDashboard();
      } else if (currentUser.role === 'Founder') {
        setActiveTab('my-project');
        fetchFounderDashboard();
      } else if (currentUser.role === 'Manager') {
        setActiveTab('vetting-queue');
        fetchManagerDashboard();
      }
    }
  }, [currentUser, activeTab]);

  const showNotification = (text: string, type: 'success' | 'error') => {
    setNotification({ text, type });
    setTimeout(() => setNotification({ text: '', type: '' }), 5000);
  };

  const fetchSession = async () => {
    try {
      const res = await fetch('/api/auth/me');
      if (res.ok) {
        const data = await res.json();
        setCurrentUser(data);
        if (data.contactLink) setContactLink(data.contactLink);
        if (data.cvUrl) setCvUrl(data.cvUrl);
      }
    } catch (err) {
      console.error('Session verify failed', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleLogin = async (emailStr: string, passwordStr: string) => {
    setIsLoading(true);
    try {
      const res = await fetch(`/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: emailStr, password: passwordStr })
      });
      if (res.ok) {
        const data = await res.json();
        setCurrentUser(data.user);
        if (data.user.contactLink) setContactLink(data.user.contactLink);
        if (data.user.cvUrl) setCvUrl(data.user.cvUrl);
        showNotification(`Đăng nhập thành công: ${data.user.name}`, 'success');
      } else {
        const errData = await res.json();
        showNotification(errData.error || 'Email hoặc Mật khẩu không chính xác.', 'error');
      }
    } catch {
      showNotification('Lỗi kết nối mạng.', 'error');
    } finally {
      setIsLoading(false);
    }
  };

  const handleRegister = async () => {
    if (!regEmail.toLowerCase().endsWith('@fpt.edu.vn')) {
      showNotification('Đăng ký yêu cầu sử dụng email @fpt.edu.vn', 'error');
      return;
    }

    setIsLoading(true);
    try {
      const res = await fetch(`/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: regEmail,
          password: regPassword,
          name: regName,
          role: regRole,
          studentId: regStudentId,
          projectName: regRole === 'Founder' ? regProjectName : '',
          projectPitch: regRole === 'Founder' ? regProjectPitch : ''
        })
      });

      if (res.ok) {
        const data = await res.json();
        setCurrentUser(data.user);
        showNotification('Đăng ký và đăng nhập thành công!', 'success');
        setIsRegister(false);
      } else {
        const errData = await res.json();
        showNotification(errData.error || 'Lỗi đăng ký tài khoản.', 'error');
      }
    } catch {
      showNotification('Lỗi kết nối mạng.', 'error');
    } finally {
      setIsLoading(false);
    }
  };

  const handleLogout = async () => {
    try {
      await fetch('/api/auth/logout', { method: 'POST' });
      setCurrentUser(null);
      setFounderProject(null);
      setProjects([]);
      setJobs([]);
      setStudentApps([]);
      setCandidates([]);
      setActiveTab('');
      setLastUploadedUrl('');
      setProjectDescription('');
      showNotification('Đã đăng xuất tài khoản.', 'success');
    } catch {
      showNotification('Lỗi đăng xuất.', 'error');
    }
  };

  // --- STUDENT DASHBOARD FETCHERS ---
  const fetchStudentDashboard = async () => {
    try {
      const resProj = await fetch('/api/projects');
      if (resProj.ok) setProjects(await resProj.ok ? await resProj.json() : []);

      const resJobs = await fetch('/api/jobs');
      if (resJobs.ok) setJobs(await resJobs.json());

      if (currentUser?.id) {
        const resApps = await fetch(`/api/applications/student/${currentUser.id}`);
        if (resApps.ok) setStudentApps(await resApps.json());
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!currentUser) return;
    setProfileMsg({ text: '', type: '' });

    try {
      const res = await fetch(`/api/users/${currentUser.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ contactLink, cvUrl })
      });

      const data = await res.json();
      if (res.ok) {
        setProfileMsg({ text: 'Hồ sơ cá nhân cập nhật thành công!', type: 'success' });
        setCurrentUser(data.user);
        // Refresh feeds
        fetchStudentDashboard();
      } else {
        setProfileMsg({ text: data.error || 'Lỗi cập nhật hồ sơ.', type: 'error' });
      }
    } catch {
      setProfileMsg({ text: 'Lỗi kết nối máy chủ.', type: 'error' });
    }
  };

  const handleApplyJob = async (jobId: string) => {
    if (!currentUser) return;
    try {
      const res = await fetch('/api/applications', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ studentId: currentUser.id, jobId })
      });
      const data = await res.json();
      if (res.ok) {
        showNotification(data.message, 'success');
        fetchStudentDashboard();
      } else {
        showNotification(data.error || 'Ứng tuyển thất bại.', 'error');
      }
    } catch {
      showNotification('Lỗi kết nối máy chủ.', 'error');
    }
  };

  // --- FOUNDER DASHBOARD FETCHERS ---
  const fetchFounderDashboard = async () => {
    if (!currentUser) return;
    try {
      // Get founder's active project details
      const userRes = await fetch('/api/auth/me');
      if (userRes.ok) {
        const userData = await userRes.json();
        setCurrentUser(userData);
        if (userData.projectId) {
          const projRes = await fetch(`/api/projects/${userData.projectId}`);
          if (projRes.ok) {
            const projData = await projRes.json();
            setFounderProject(projData);
            setProjectPitch(projData.pitch);
            setProjectDescription(projData.description || '');
            setProjectDemoUrl(projData.demoUrl || '');

            // Fetch job applicants for this project
            const candRes = await fetch(`/api/applications/project/${projData.id}`);
            if (candRes.ok) setCandidates(await candRes.json());
          }
        }
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleSaveProjectInfo = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!founderProject) return;
    try {
      const res = await fetch(`/api/projects/${founderProject.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          pitch: projectPitch, 
          description: projectDescription,
          demoUrl: projectDemoUrl 
        })
      });
      const data = await res.json();
      if (res.ok) {
        showNotification('Cập nhật thông tin dự án thành công!', 'success');
        fetchFounderDashboard();
      } else {
        showNotification(data.error || 'Cập nhật thất bại.', 'error');
      }
    } catch {
      showNotification('Lỗi kết nối khi cập nhật dự án.', 'error');
    }
  };

  const handleAddMilestone = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!founderProject || !newMilestoneTitle) return;

    try {
      const res = await fetch(`/api/projects/${founderProject.id}/milestones`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: newMilestoneTitle, description: newMilestoneDesc })
      });
      const data = await res.json();
      if (res.ok) {
        showNotification(data.message, 'success');
        setNewMilestoneTitle('');
        setNewMilestoneDesc('');
        fetchFounderDashboard();
      } else {
        showNotification(data.error || 'Lỗi thêm cột mốc.', 'error');
      }
    } catch {
      showNotification('Lỗi kết nối.', 'error');
    }
  };

  const handlePostJob = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!founderProject || !newJobTitle || !newJobDesc) return;

    try {
      const res = await fetch('/api/jobs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          projectId: founderProject.id,
          title: newJobTitle,
          category: newJobCategory,
          description: newJobDesc,
          requirements: newJobReqs
        })
      });
      if (res.ok) {
        showNotification('Đăng tuyển dụng thành công!', 'success');
        setNewJobTitle('');
        setNewJobDesc('');
        setNewJobReqs('');
        fetchFounderDashboard();
      } else {
        const data = await res.json();
        showNotification(data.error || 'Đăng tuyển thất bại.', 'error');
      }
    } catch {
      showNotification('Lỗi mạng.', 'error');
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!founderProject || !e.target.files || e.target.files.length === 0) return;
    const file = e.target.files[0];
    
    setUploadProgress(true);
    setUploadMsg({ text: '', type: '' });

    const formData = new FormData();
    formData.append('file', file);

    try {
      const res = await fetch(`/api/projects/${founderProject.id}/upload`, {
        method: 'POST',
        body: formData
      });
      const data = await res.json();
      if (res.ok) {
        setLastUploadedUrl(data.fileUrl);
        setUploadMsg({ text: 'Tải lên tài liệu thành công!', type: 'success' });
        fetchFounderDashboard();
      } else {
        setUploadMsg({ text: data.error || 'Tải lên thất bại.', type: 'error' });
      }
    } catch {
      setUploadMsg({ text: 'Lỗi mạng khi tải lên.', type: 'error' });
    } finally {
      setUploadProgress(false);
    }
  };

  const handleReviewCandidate = async (appId: string, status: 'Approved' | 'Rejected') => {
    try {
      const res = await fetch(`/api/applications/${appId}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status })
      });
      const data = await res.json();
      if (res.ok) {
        showNotification(data.message, 'success');
        fetchFounderDashboard();
      } else {
        showNotification(data.error || 'Lỗi phê duyệt.', 'error');
      }
    } catch {
      showNotification('Lỗi mạng.', 'error');
    }
  };

  const handleRemoveMember = async (memberId: string) => {
    if (!founderProject) return;
    if (!window.confirm('Bạn có chắc chắn muốn loại bỏ thành viên này khỏi dự án?')) return;
    try {
      const res = await fetch(`/api/projects/${founderProject.id}/members/${memberId}`, {
        method: 'DELETE'
      });
      const data = await res.json();
      if (res.ok) {
        showNotification('Đã loại bỏ thành viên ra khỏi dự án.', 'success');
        fetchFounderDashboard();
      } else {
        showNotification(data.error || 'Loại bỏ thất bại.', 'error');
      }
    } catch {
      showNotification('Lỗi kết nối khi loại bỏ thành viên.', 'error');
    }
  };

  // --- MANAGER DASHBOARD FETCHERS ---
  const fetchManagerDashboard = async () => {
    try {
      const res = await fetch('/api/projects');
      if (res.ok) setProjects(await res.json());
    } catch (err) {
      console.error(err);
    }
  };

  const handleVetProject = async (projectId: string, status: 'Active' | 'Suspended') => {
    try {
      const res = await fetch(`/api/admin/projects/${projectId}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status })
      });
      const data = await res.json();
      if (res.ok) {
        showNotification(data.message, 'success');
        fetchManagerDashboard();
      } else {
        showNotification(data.error || 'Lỗi duyệt.', 'error');
      }
    } catch {
      showNotification('Lỗi mạng.', 'error');
    }
  };

  const handleRunDormancyCheck = async () => {
    try {
      const res = await fetch('/api/admin/run-dormancy-check', { method: 'POST' });
      const data = await res.json();
      if (res.ok) {
        showNotification(
          `Quét hoàn tất! Đã cập nhật ${data.results.warningsIssued} cảnh báo, tạm ngưng ${data.results.suspendedCount} dự án.`,
          'success'
        );
        fetchManagerDashboard();
      } else {
        showNotification(data.error || 'Chạy quét thất bại.', 'error');
      }
    } catch {
      showNotification('Lỗi mạng.', 'error');
    }
  };

  // --- FORMAT STORAGE SIZE ---
  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = 2;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#0a0a16]">
        <div className="flex flex-col items-center gap-4">
          <div className="w-12 h-12 border-4 border-purple-500 border-t-transparent rounded-full animate-spin"></div>
          <p className="font-heading text-purple-400 animate-pulse">Đang nạp hệ thống Gara...</p>
        </div>
      </div>
    );
  }

  // --- LOGIN GATEVIEW ---
  if (!currentUser) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4 relative">
        {/* Glow Effects */}
        <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-purple-500/10 rounded-full blur-3xl"></div>
        <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-cyan-500/10 rounded-full blur-3xl"></div>

        <div className="glass-panel p-8 max-w-md w-full relative z-10 border-purple-500/20">
          <div className="flex justify-center mb-6">
            <div className="w-16 h-16 rounded-2xl bg-gradient-to-tr from-purple-600 to-cyan-500 flex items-center justify-center shadow-lg shadow-purple-500/20">
              <Layers className="w-9 h-9 text-white" />
            </div>
          </div>
          
          <h1 className="text-3xl font-extrabold font-heading bg-gradient-to-r from-purple-400 to-cyan-400 bg-clip-text text-transparent text-center mb-2">
            GARA PORTAL
          </h1>
          <p className="text-sm text-slate-400 text-center mb-4 font-body">
            Hệ thống quản lý dự án & tuyển dụng Vườn ươm khởi nghiệp trường Đại học.
          </p>

          <div className="flex justify-center mb-6">
            <a 
              href="/mockup.html" 
              target="_blank" 
              rel="noopener noreferrer" 
              className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-cyan-500/10 text-cyan-400 border border-cyan-500/20 text-xs font-semibold hover:bg-cyan-500/20 transition-all font-heading"
            >
              <FileText className="w-3.5 h-3.5" />
              Xem Showcase dự án mẫu trực quan ↗
            </a>
          </div>

          {!isRegister ? (
            <>
              <form onSubmit={(e) => { e.preventDefault(); handleLogin(loginEmail, loginPassword); }} className="space-y-4 mb-6">
                <div>
                  <label className="block text-left text-xs font-semibold text-slate-400 mb-1.5 uppercase tracking-wider">Email học viên / Quản trị</label>
                  <input
                    type="email"
                    placeholder="email@fpt.edu.vn"
                    value={loginEmail}
                    onChange={(e) => setLoginEmail(e.target.value)}
                    required
                    className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-3 text-slate-200"
                  />
                </div>
                
                <div>
                  <label className="block text-left text-xs font-semibold text-slate-400 mb-1.5 uppercase tracking-wider">Mật khẩu</label>
                  <input
                    type="password"
                    placeholder="••••••••"
                    value={loginPassword}
                    onChange={(e) => setLoginPassword(e.target.value)}
                    required
                    className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-3 text-slate-200"
                  />
                </div>

                <button
                  type="submit"
                  disabled={isLoading}
                  className="btn btn-primary w-full py-3.5 mt-2 rounded-lg font-heading"
                >
                  {isLoading ? 'Đang xác thực...' : 'Đăng Nhập'}
                </button>
              </form>

              <div className="text-center mb-6">
                <button 
                  onClick={() => setIsRegister(true)} 
                  className="text-xs text-purple-400 hover:text-purple-300 font-semibold"
                >
                  Chưa có tài khoản? Đăng ký ngay ↗
                </button>
              </div>

              <div className="border-t border-white/5 pt-4">
                <span className="block text-left text-xs font-semibold text-slate-400 mb-3 uppercase tracking-wider">Tài khoản kiểm thử nhanh (Click để tự điền & đăng nhập)</span>
                <div className="flex flex-col gap-2 max-h-[160px] overflow-y-auto pr-1">
                  {[
                    { name: 'Hỷ Minh Phát', id: 'SE184629', email: 'phathmse184629@fpt.edu.vn', role: 'Sáng lập Gara Showcase', color: 'text-purple-400' },
                    { name: 'Lê Tuấn Khanh', id: 'SE184638', email: 'khanhltse184638@fpt.edu.vn', role: 'Sinh viên ứng tuyển', color: 'text-cyan-400' },
                    { name: 'Trương Ngọc Vy', id: 'SE182233', email: 'vyntse182233@fpt.edu.vn', role: 'Sinh viên ứng tuyển', color: 'text-cyan-400' },
                    { name: 'Trần Minh Quân', id: 'SE183204', email: 'edulink.founder@fpt.edu.vn', role: 'Sáng lập EduLink NFC', color: 'text-purple-400' },
                    { name: 'Võ Hoàng Yến', id: 'SE185112', email: 'zodiac.founder@fpt.edu.vn', role: 'Sáng lập Zodiac AI', color: 'text-purple-400' },
                    { name: 'Vườn Ươm Gara Manager', id: 'Admin', email: 'manager.mock@fpt.edu.vn', role: 'Quản trị hệ thống', color: 'text-rose-400' }
                  ].map((acc) => (
                    <button
                      key={acc.email}
                      type="button"
                      onClick={() => {
                        setLoginEmail(acc.email);
                        setLoginPassword('password123');
                        handleLogin(acc.email, 'password123');
                      }}
                      className="btn btn-outline text-left w-full flex flex-col justify-center px-4 py-2.5 glass-panel-interactive border-white/10"
                    >
                      <div className="flex justify-between items-center w-full">
                        <span className="font-heading font-semibold text-xs text-slate-200">{acc.name}</span>
                        <span className={`text-[10px] font-mono ${acc.color}`}>{acc.role}</span>
                      </div>
                      <span className="text-[10px] text-slate-400 mt-0.5">{acc.email}</span>
                    </button>
                  ))}
                </div>
              </div>
            </>
          ) : (
            <>
              <form onSubmit={(e) => { e.preventDefault(); handleRegister(); }} className="space-y-4 mb-6 text-left">
                <div>
                  <label className="block text-xs font-semibold text-slate-400 mb-1.5 uppercase tracking-wider">Họ và Tên</label>
                  <input
                    type="text"
                    placeholder="Nguyễn Văn A"
                    value={regName}
                    onChange={(e) => setRegName(e.target.value)}
                    required
                    className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-slate-200 text-sm"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-400 mb-1.5 uppercase tracking-wider">Email (Yêu cầu @fpt.edu.vn)</label>
                  <input
                    type="email"
                    placeholder="anvse123456@fpt.edu.vn"
                    value={regEmail}
                    onChange={(e) => setRegEmail(e.target.value)}
                    required
                    className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-slate-200 text-sm"
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-semibold text-slate-400 mb-1.5 uppercase tracking-wider">Mật khẩu</label>
                    <input
                      type="password"
                      placeholder="••••••••"
                      value={regPassword}
                      onChange={(e) => setRegPassword(e.target.value)}
                      required
                      className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-slate-200 text-sm"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-slate-400 mb-1.5 uppercase tracking-wider">Mã số sinh viên</label>
                    <input
                      type="text"
                      placeholder="SE123456"
                      value={regStudentId}
                      onChange={(e) => setRegStudentId(e.target.value)}
                      required
                      className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-slate-200 text-sm"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-400 mb-1.5 uppercase tracking-wider">Vai trò đăng ký</label>
                  <div className="grid grid-cols-2 gap-2 mt-1">
                    <button
                      type="button"
                      onClick={() => setRegRole('Student')}
                      className={`py-2.5 px-3 rounded-lg border text-xs font-semibold font-heading transition-all ${
                        regRole === 'Student'
                          ? 'bg-cyan-500/10 text-cyan-400 border-cyan-500/30'
                          : 'bg-white/5 text-slate-400 border-white/5 hover:bg-white/10'
                      }`}
                    >
                      Sinh viên tìm dự án
                    </button>
                    <button
                      type="button"
                      onClick={() => setRegRole('Founder')}
                      className={`py-2.5 px-3 rounded-lg border text-xs font-semibold font-heading transition-all ${
                        regRole === 'Founder'
                          ? 'bg-purple-500/10 text-purple-400 border-purple-500/30'
                          : 'bg-white/5 text-slate-400 border-white/5 hover:bg-white/10'
                      }`}
                    >
                      Nhà sáng lập (Founder)
                    </button>
                  </div>
                </div>

                {regRole === 'Founder' && (
                  <div className="space-y-4 border-t border-purple-500/10 pt-4 animate-fade-in">
                    <div>
                      <label className="block text-xs font-semibold text-purple-400 mb-1.5 uppercase tracking-wider">Tên dự án khởi nghiệp</label>
                      <input
                        type="text"
                        placeholder="Ví dụ: Gara Eco-Cycle"
                        value={regProjectName}
                        onChange={(e) => setRegProjectName(e.target.value)}
                        required={regRole === 'Founder'}
                        className="w-full bg-white/5 border border-purple-500/20 rounded-lg px-4 py-2.5 text-slate-200 text-sm"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold text-purple-400 mb-1.5 uppercase tracking-wider">Ý tưởng cốt lõi (Pitch ngắn)</label>
                      <input
                        type="text"
                        placeholder="Nền tảng thu gom rác tái chế bằng ứng dụng..."
                        value={regProjectPitch}
                        onChange={(e) => setRegProjectPitch(e.target.value)}
                        required={regRole === 'Founder'}
                        className="w-full bg-white/5 border border-purple-500/20 rounded-lg px-4 py-2.5 text-slate-200 text-sm"
                      />
                    </div>
                  </div>
                )}

                <button
                  type="submit"
                  disabled={isLoading}
                  className="btn btn-primary w-full py-3.5 mt-2 rounded-lg font-heading"
                >
                  {isLoading ? 'Đang đăng ký...' : 'Hoàn Tất Đăng Ký & Đăng Nhập'}
                </button>
              </form>

              <div className="text-center">
                <button 
                  onClick={() => setIsRegister(false)} 
                  className="text-xs text-slate-400 hover:text-slate-300 font-semibold"
                >
                  ← Đã có tài khoản? Đăng nhập tại đây
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col">
      {/* Toast Notification */}
      {notification.text && (
        <div className={`fixed top-6 right-6 z-50 px-6 py-3 rounded-lg shadow-lg flex items-center gap-3 font-heading text-sm border animate-fade-in ${
          notification.type === 'success' 
            ? 'bg-emerald-950/80 text-emerald-400 border-emerald-500/30' 
            : 'bg-rose-950/80 text-rose-400 border-rose-500/30'
        }`}>
          {notification.type === 'success' ? <CheckCircle className="w-5 h-5" /> : <AlertTriangle className="w-5 h-5" />}
          {notification.text}
        </div>
      )}

      {/* --- HEADER --- */}
      <header className="glass-panel border-t-0 border-x-0 rounded-none px-6 py-4 flex items-center justify-between sticky top-0 z-40 bg-[#070712]/90 backdrop-blur-md">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-purple-600 to-cyan-500 flex items-center justify-center">
            <Layers className="w-6 h-6 text-white" />
          </div>
          <div>
            <h1 className="font-heading font-bold text-lg leading-tight bg-gradient-to-r from-purple-400 to-cyan-400 bg-clip-text text-transparent">
              GARA SHOWCASE
            </h1>
            <p className="text-[10px] text-slate-400 tracking-wider font-body">INNOVATION HUB</p>
          </div>
          <div className="hidden sm:block ml-4 border-l border-white/10 pl-4">
            <a 
              href="/mockup.html" 
              target="_blank" 
              rel="noopener noreferrer" 
              className="inline-flex items-center gap-1.5 text-xs text-slate-400 hover:text-cyan-400 transition-colors font-body"
            >
              <FileText className="w-3.5 h-3.5" />
              Showcase dự án mẫu ↗
            </a>
          </div>
        </div>

        <div className="flex items-center gap-6">
          <div className="flex items-center gap-3 px-3 py-1.5 rounded-lg bg-white/5 border border-white/5">
            <div className="w-8 h-8 rounded-full bg-purple-500/20 border border-purple-500/30 flex items-center justify-center">
              <UserIcon className="w-4 h-4 text-purple-400" />
            </div>
            <div className="text-left hidden md:block">
              <p className="text-xs font-semibold leading-none text-slate-200">{currentUser.name}</p>
              <p className="text-[10px] text-purple-400 font-heading font-medium mt-0.5 uppercase tracking-wider">{currentUser.role}</p>
            </div>
          </div>

          <button 
            onClick={handleLogout}
            className="btn btn-outline py-2 px-3 text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 border-white/5 flex items-center gap-2"
          >
            <LogOut className="w-4 h-4" />
            <span className="text-xs hidden md:inline">Đăng xuất</span>
          </button>
        </div>
      </header>

      {/* --- PORTALS ROUTER WRAPPER --- */}
      <main className="flex-1 max-w-7xl w-full mx-auto p-4 md:p-6 animate-fade-in">
        
        {/* ================= STUDENT PORTAL ================= */}
        {currentUser.role === 'Student' && (
          <div>
            <div className="tabs-container">
              <button 
                onClick={() => { setActiveTab('projects'); fetchStudentDashboard(); }} 
                className={`tab-btn ${activeTab === 'projects' ? 'active' : ''}`}
              >
                Khám phá Dự án
              </button>
              <button 
                onClick={() => { setActiveTab('jobs'); fetchStudentDashboard(); }} 
                className={`tab-btn ${activeTab === 'jobs' ? 'active' : ''}`}
              >
                Cơ hội Tuyển dụng
              </button>
              <button 
                onClick={() => { setActiveTab('my-joined-project'); fetchStudentDashboard(); }} 
                className={`tab-btn ${activeTab === 'my-joined-project' ? 'active' : ''}`}
              >
                Dự án tham gia
              </button>
              <button 
                onClick={() => { setActiveTab('history'); fetchStudentDashboard(); }} 
                className={`tab-btn ${activeTab === 'history' ? 'active' : ''}`}
              >
                Lịch sử & Profile
              </button>
            </div>

            {/* TAB: Joined Project */}
            {activeTab === 'my-joined-project' && (() => {
              if (!currentUser.projectId) {
                return (
                  <div className="glass-panel p-8 text-center max-w-2xl mx-auto my-12 space-y-4 border-purple-500/20">
                    <div className="w-16 h-16 rounded-full bg-purple-500/10 border border-purple-500/20 flex items-center justify-center mx-auto text-purple-400">
                      <Users className="w-8 h-8" />
                    </div>
                    <h3 className="text-xl font-heading font-bold text-slate-200">Bạn chưa tham gia dự án nào</h3>
                    <p className="text-sm text-slate-400 max-w-md mx-auto">
                      Để xem chi tiết lộ trình, quản lý dung lượng và xem danh sách đồng đội, bạn cần là thành viên chính thức của một dự án khởi nghiệp thuộc vườn ươm.
                    </p>
                    <div className="pt-2">
                      <button 
                        onClick={() => { setActiveTab('jobs'); fetchStudentDashboard(); }}
                        className="btn btn-primary px-6 py-2.5 rounded-lg text-xs font-semibold font-heading"
                      >
                        Ứng tuyển thành viên ngay
                      </button>
                    </div>
                  </div>
                );
              }
              const myProject = projects.find(p => p.id === currentUser.projectId);
              if (!myProject) return <p className="text-slate-500 py-8 text-center">Đang đồng bộ thông tin dự án của bạn...</p>;
              return (
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 text-left animate-fade-in mt-6">
                  {/* Left Column: Project Profile */}
                  <div className="glass-panel p-6 border-white/5 space-y-6 lg:col-span-1 h-fit">
                    <div>
                      <span className={`badge ${
                        myProject.status === 'Active' ? 'badge-active' :
                        myProject.status === 'Suspended' ? 'badge-suspended' : 'badge-draft'
                      } mb-2`}>{myProject.status}</span>
                      <h2 className="text-2xl font-heading font-extrabold text-slate-100">{myProject.name}</h2>
                      <p className="text-sm text-slate-400 mt-2 italic">"{myProject.pitch}"</p>

                      {myProject.demoUrl && (
                        <div className="mt-4">
                          <a 
                            href={myProject.demoUrl} 
                            target="_blank" 
                            rel="noopener noreferrer" 
                            className="btn btn-outline py-2 px-4 w-full text-xs font-semibold text-center border-cyan-500/20 text-cyan-400 hover:bg-cyan-500/10 flex items-center justify-center gap-1.5"
                          >
                            <FileText className="w-3.5 h-3.5" />
                            Xem Showcase Trực quan ↗
                          </a>
                        </div>
                      )}
                    </div>

                    <div className="space-y-2 border-t border-white/5 pt-4 text-xs">
                      <span className="text-slate-400 font-bold uppercase block mb-1">Dung lượng lưu trữ</span>
                      <div className="flex justify-between text-slate-300">
                        <span>Đã sử dụng:</span>
                        <span className="font-mono">{formatBytes(myProject.storageUsedBytes)} / 500 MB</span>
                      </div>
                      <div className="w-full bg-white/5 h-1.5 rounded-full overflow-hidden mt-1">
                        <div 
                          className="bg-purple-500 h-full"
                          style={{ width: `${Math.min(100, (myProject.storageUsedBytes / (500 * 1024 * 1024)) * 100)}%` }}
                        ></div>
                      </div>
                    </div>
                  </div>

                  {/* Right Columns: Description, Team Roster, and Milestones */}
                  <div className="lg:col-span-2 space-y-6">
                    {/* Project Description (Markdown) */}
                    <div className="glass-panel p-6 border-white/5 space-y-4">
                      <h3 className="text-lg font-heading font-bold text-slate-200 flex items-center gap-2">
                        <FileText className="w-5 h-5 text-purple-400" /> Mô tả chi tiết dự án
                      </h3>
                      <div className="p-4 rounded-lg bg-white/5 border border-white/5 max-h-[300px] overflow-y-auto text-left">
                        {renderMarkdown(myProject.description)}
                      </div>
                    </div>

                    {/* Teammates List */}
                    <div className="glass-panel p-6 border-white/5 space-y-4">
                      <h3 className="text-lg font-heading font-bold text-slate-200 flex items-center gap-2">
                        <Users className="w-5 h-5 text-purple-400" /> Đồng đội của tôi
                      </h3>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {myProject.teamMembers.map(m => (
                          <div key={m.id} className="p-3.5 rounded-lg bg-white/5 border border-white/5 flex items-center justify-between">
                            <div>
                              <h4 className="font-heading font-bold text-sm text-slate-200">
                                {m.name} {m.role === 'Founder' && <span className="text-[9px] bg-purple-500/20 text-purple-400 border border-purple-500/30 px-1.5 py-0.5 rounded font-mono ml-1 font-normal">FOUNDER</span>}
                              </h4>
                              <p className="text-[10px] text-slate-400 mt-1">{m.studentId} • {m.email}</p>
                            </div>
                            {m.contactLink && (
                              <a href={m.contactLink} target="_blank" rel="noreferrer" className="text-slate-400 hover:text-cyan-400">
                                <LinkIcon className="w-4 h-4" />
                              </a>
                            )}
                          </div>
                        ))}
                      </div>
                    </div>

                    {/* Milestones list */}
                    <div className="glass-panel p-6 border-white/5 space-y-4">
                      <h3 className="text-lg font-heading font-bold text-slate-200 flex items-center gap-2">
                        <CheckCircle className="w-5 h-5 text-cyan-400" /> Lộ trình & Cột mốc hoàn thành
                      </h3>
                      <div className="space-y-3">
                        {myProject.milestones.length === 0 ? (
                          <p className="text-xs text-slate-500">Chưa ghi nhận cột mốc nào.</p>
                        ) : (
                          myProject.milestones.map(m => (
                            <div key={m.id} className="p-3.5 rounded-lg bg-white/5 border border-white/5 flex items-center justify-between">
                              <div className="flex items-center gap-3">
                                {m.done ? (
                                  <CheckSquare className="w-5 h-5 text-emerald-400 shrink-0" />
                                ) : (
                                  <div className="w-5 h-5 border-2 border-slate-500 rounded shrink-0"></div>
                                )}
                                <span className={`text-sm ${m.done ? 'line-through text-slate-500' : 'text-slate-200'}`}>{m.title}</span>
                              </div>
                              {m.done && m.dateCompleted && (
                                <span className="text-[10px] text-emerald-400">Hoàn thành: {new Date(m.dateCompleted).toLocaleDateString()}</span>
                              )}
                            </div>
                          ))
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              );
            })()}

            {/* TAB: Projects */}
            {activeTab === 'projects' && (
              <div className="space-y-6">
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                  <div>
                    <h2 className="text-2xl font-heading font-bold text-slate-200">Startup Portfolio</h2>
                    <p className="text-sm text-slate-400">Khám phá danh sách các ý tưởng và dự án khởi nghiệp sinh viên.</p>
                  </div>
                  <div className="relative max-w-sm w-full">
                    <Search className="absolute w-4 h-4 text-slate-500" style={{ top: '50%', transform: 'translateY(-50%)', left: '12px' }} />
                    <input 
                      type="text" 
                      placeholder="Tìm kiếm dự án..." 
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      className="pl-9 py-2.5"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {projects.filter(p => p.name.toLowerCase().includes(searchQuery.toLowerCase())).map(p => (
                    <div key={p.id} className="glass-panel glass-panel-interactive p-6 flex flex-col justify-between border-white/5">
                      <div>
                        <div className="flex items-start justify-between mb-3">
                          <span className={`badge ${
                            p.status === 'Active' ? 'badge-active' : 'badge-risk'
                          }`}>{p.status}</span>
                          <span className="text-xs text-slate-500 font-body">Cập nhật: {new Date(p.lastUpdatedAt).toLocaleDateString()}</span>
                        </div>
                        <h3 className="text-xl font-heading font-bold text-slate-100 mb-2">{p.name}</h3>
                        <p className="text-sm text-slate-400 mb-4 line-clamp-2">{p.pitch}</p>
                      </div>

                      <div className="border-t border-white/5 pt-4 flex items-center justify-between">
                        <div className="flex items-center gap-4 text-xs text-slate-400">
                          <span className="flex items-center gap-1.5"><CheckCircle className="w-4 h-4 text-purple-400" /> {p.milestones.length} Cột mốc</span>
                          <span className="flex items-center gap-1.5"><Users className="w-4 h-4 text-cyan-400" /> {p.teamMembers.length} Thành viên</span>
                        </div>
                        <button 
                          onClick={() => setSelectedProject(p)}
                          className="btn btn-outline py-1.5 px-3 text-xs border-purple-500/25 text-purple-400 hover:bg-purple-500/10"
                        >
                          Chi tiết
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* TAB: Jobs */}
            {activeTab === 'jobs' && (
              <div className="space-y-6">
                <div>
                  <h2 className="text-2xl font-heading font-bold text-slate-200">Tuyển dụng thành viên</h2>
                  <p className="text-sm text-slate-400">Gia nhập các startup của trường đại học, ứng tuyển vào các vai trò liên khoa.</p>
                </div>

                {(!currentUser.contactLink || !currentUser.cvUrl) && (
                  <div className="p-4 rounded-lg bg-amber-950/40 border border-amber-500/30 text-amber-400 text-sm flex gap-3">
                    <AlertTriangle className="w-5 h-5 shrink-0" />
                    <div>
                      <span className="font-bold">Hồ sơ chưa hoàn thiện:</span> Bạn bắt buộc phải cập nhật thông tin liên hệ và đính kèm CV PDF tại tab <strong>Lịch sử & Profile</strong> mới có thể bấm nộp đơn tuyển dụng.
                    </div>
                  </div>
                )}

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {jobs.map(j => (
                    <div key={j.id} className="glass-panel p-6 flex flex-col justify-between border-white/5">
                      <div>
                        <div className="flex items-center justify-between mb-3">
                          <span className="badge badge-draft">{j.category}</span>
                          <span className="text-xs text-slate-400 font-heading font-bold">{j.project?.name}</span>
                        </div>
                        <h3 className="text-lg font-heading font-bold text-slate-100 mb-2">{j.title}</h3>
                        <p className="text-sm text-slate-300 mb-4">{j.description}</p>
                        
                        <div className="bg-white/5 rounded-lg p-3 border border-white/5 mb-6 text-xs text-slate-400">
                          <span className="font-bold text-slate-300 block mb-1">Yêu cầu vị trí:</span>
                          {j.requirements}
                        </div>
                      </div>

                      <button 
                        onClick={() => handleApplyJob(j.id)}
                        disabled={!currentUser.contactLink || !currentUser.cvUrl}
                        className="btn btn-primary w-full disabled:opacity-50 disabled:pointer-events-none"
                      >
                        Nộp đơn ứng tuyển (CV)
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* TAB: Profile & History */}
            {activeTab === 'history' && (
              <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Profile update form */}
                <div className="glass-panel p-6 border-white/5 space-y-6">
                  <div>
                    <h3 className="text-lg font-heading font-bold text-slate-200">Hồ sơ cá nhân</h3>
                    <p className="text-xs text-slate-400">Cung cấp liên kết liên hệ và hồ sơ năng lực của bạn.</p>
                  </div>

                  {profileMsg.text && (
                    <div className={`p-3 rounded text-xs border ${
                      profileMsg.type === 'success' ? 'bg-emerald-950/40 border-emerald-500/30 text-emerald-400' : 'bg-rose-950/40 border-rose-500/30 text-rose-400'
                    }`}>
                      {profileMsg.text}
                    </div>
                  )}

                  <form onSubmit={handleUpdateProfile} className="space-y-4">
                    <div className="space-y-1">
                      <label className="text-xs text-slate-400 font-bold uppercase">Mã số Sinh viên</label>
                      <input type="text" value={currentUser.studentId || ''} disabled className="opacity-50 cursor-not-allowed" />
                    </div>

                    <div className="space-y-1">
                      <label className="text-xs text-slate-400 font-bold uppercase">Liên kết liên hệ (Facebook / LinkedIn)</label>
                      <input 
                        type="url" 
                        value={contactLink}
                        onChange={(e) => setContactLink(e.target.value)}
                        placeholder="https://facebook.com/..." 
                        required
                      />
                    </div>

                    <div className="space-y-1">
                      <label className="text-xs text-slate-400 font-bold uppercase">Liên kết tệp hồ sơ CV (PDF)</label>
                      <input 
                        type="url" 
                        value={cvUrl}
                        onChange={(e) => setCvUrl(e.target.value)}
                        placeholder="https://..." 
                        required
                      />
                    </div>

                    <button type="submit" className="btn btn-secondary w-full">Lưu thông tin</button>
                  </form>

                  {currentUser.projectId && (
                    <div className="mt-4 p-4 rounded-lg bg-purple-950/30 border border-purple-500/30 text-purple-400 text-xs">
                      <span className="font-bold block mb-1">Dự án tham gia:</span>
                      Đang liên kết với dự án thành viên.
                    </div>
                  )}
                </div>

                {/* Applications history */}
                <div className="glass-panel p-6 border-white/5 lg:col-span-2 space-y-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="text-lg font-heading font-bold text-slate-200">Đơn ứng tuyển của tôi</h3>
                      <p className="text-xs text-slate-400">Danh sách các cơ hội mà bạn đã nộp đơn tuyển dụng.</p>
                    </div>
                    <span className="text-xs badge badge-draft">
                      {studentApps.filter(a => a.application_status === 'Pending').length} / 3 Chờ duyệt
                    </span>
                  </div>

                  <div className="space-y-4">
                    {studentApps.length === 0 ? (
                      <p className="text-sm text-slate-500 text-center py-8">Bạn chưa nộp đơn ứng tuyển nào.</p>
                    ) : (
                      studentApps.map(a => (
                        <div key={a.id} className="p-4 rounded-lg bg-white/5 border border-white/5 flex items-center justify-between">
                          <div>
                            <h4 className="font-heading font-bold text-sm text-slate-200">{a.job_title}</h4>
                            <p className="text-xs text-slate-400 mt-1">{a.project_name}</p>
                          </div>
                          <div className="text-right">
                            <span className={`badge text-[10px] ${
                              a.application_status === 'Approved' ? 'badge-active' :
                              a.application_status === 'Rejected' ? 'badge-suspended' : 'badge-draft'
                            }`}>{a.application_status}</span>
                            <p className="text-[10px] text-slate-500 mt-1.5">{new Date(a.createdAt).toLocaleDateString()}</p>
                          </div>
                        </div>
                      ))
                    )}
                  </div>
                </div>
              </div>
            )}
          </div>
        )}

        {/* ================= FOUNDER PORTAL ================= */}
        {currentUser.role === 'Founder' && (
          <div>
            {!currentUser.projectId ? (
              <div className="glass-panel p-8 max-w-lg mx-auto text-center border-amber-500/20">
                <AlertTriangle className="w-12 h-12 text-amber-500 mx-auto mb-4" />
                <h2 className="text-xl font-heading font-bold text-slate-200 mb-2">Chưa gán quyền sở hữu Startup</h2>
                <p className="text-sm text-slate-400 mb-6">
                  Bạn đăng nhập bằng quyền Nhà sáng lập nhưng tài khoản của bạn chưa liên kết với bất cứ dự án nào. Vui lòng liên hệ với Vườn ươm quản trị để được thêm quyền.
                </p>
              </div>
            ) : !founderProject ? (
              <p className="text-center text-slate-500">Đang đồng bộ dữ liệu dự án của sáng lập viên...</p>
            ) : (
              <div>
                <div className="tabs-container">
                  <button 
                    onClick={() => { setActiveTab('my-project'); fetchFounderDashboard(); }} 
                    className={`tab-btn ${activeTab === 'my-project' ? 'active' : ''}`}
                  >
                    Dự án của tôi
                  </button>
                  <button 
                    onClick={() => { setActiveTab('recruits'); fetchFounderDashboard(); }} 
                    className={`tab-btn ${activeTab === 'recruits' ? 'active' : ''}`}
                  >
                    Ứng viên tuyển dụng ({candidates.filter(c => c.status === 'Pending').length})
                  </button>
                </div>

                {/* TAB: My Project Editor & Milestones */}
                {activeTab === 'my-project' && (
                  <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* General Profile Info & storage */}
                    <div className="glass-panel p-6 border-white/5 space-y-6">
                      <form onSubmit={handleSaveProjectInfo} className="space-y-4">
                        <div className="space-y-1">
                          <label className="text-xs text-slate-400 font-bold uppercase">Tên Startup</label>
                          <input type="text" value={founderProject.name} disabled className="opacity-50 cursor-not-allowed" />
                        </div>

                        <div className="space-y-1">
                          <label className="text-xs text-slate-400 font-bold uppercase">Khẩu hiệu (Pitch)</label>
                          <textarea 
                            rows={2} 
                            value={projectPitch} 
                            onChange={(e) => setProjectPitch(e.target.value)} 
                            placeholder="Nhập giới thiệu ngắn gọn về dự án..."
                            required
                          />
                        </div>

                        <div className="space-y-1 text-left">
                          <label className="text-xs text-slate-400 font-bold uppercase">Link Showcase / Demo Trực quan</label>
                          <input 
                            type="text" 
                            value={projectDemoUrl} 
                            onChange={(e) => setProjectDemoUrl(e.target.value)} 
                            placeholder="https://figma.com/... hoặc link website chạy thử"
                            className="w-full bg-white/5 border border-white/10 rounded-lg px-3 py-2 text-slate-200 text-xs"
                          />
                        </div>

                        <div className="space-y-1">
                          <label className="text-xs text-slate-400 font-bold uppercase">Mô tả chi tiết (Markdown)</label>
                          <textarea 
                            rows={8} 
                            value={projectDescription} 
                            onChange={(e) => setProjectDescription(e.target.value)} 
                            placeholder="Nhập mô tả chi tiết bằng định dạng Markdown (hỗ trợ liên kết tài liệu, hình ảnh)..."
                            required
                          />
                        </div>

                        <button type="submit" className="btn btn-secondary w-full py-2.5">
                          Lưu thông tin dự án
                        </button>
                      </form>

                      {/* Quota storage footprint */}
                      <div className="space-y-2 border-t border-white/5 pt-4">
                        <div className="flex justify-between text-xs font-heading font-semibold">
                          <span className="text-slate-400">Không gian lưu trữ (Quotas)</span>
                          <span className="text-slate-300">
                            {formatBytes(founderProject.storageUsedBytes)} / 500 MB
                          </span>
                        </div>
                        <div className="w-full bg-white/5 h-2 rounded-full overflow-hidden border border-white/5">
                          <div 
                            className="bg-purple-500 h-full transition-all duration-300"
                            style={{ width: `${Math.min(100, (founderProject.storageUsedBytes / (500 * 1024 * 1024)) * 100)}%` }}
                          ></div>
                        </div>
                        <p className="text-[10px] text-slate-500">Giới hạn tải lên tối đa 10MB cho mỗi tệp tin đính kèm.</p>
                      </div>

                      {/* File uploader stream tool */}
                      <div className="border-t border-white/5 pt-4 space-y-4">
                        <label className="text-xs text-slate-300 font-bold block">Tải lên tệp tài liệu dự án (.pdf, .zip)</label>
                        
                        {uploadMsg.text && (
                          <div className="space-y-2">
                            <div className={`p-3 rounded text-xs border ${
                              uploadMsg.type === 'success' ? 'bg-emerald-950/40 border-emerald-500/30 text-emerald-400' : 'bg-rose-950/40 border-rose-500/30 text-rose-400'
                            }`}>{uploadMsg.text}</div>
                            {uploadMsg.type === 'success' && lastUploadedUrl && (
                              <div className="p-3 rounded bg-white/5 border border-white/10 text-[10px] space-y-1.5 text-left">
                                <span className="font-bold text-slate-300 block">Đường dẫn tệp tài liệu:</span>
                                <input 
                                  type="text" 
                                  readOnly 
                                  value={lastUploadedUrl} 
                                  className="w-full bg-black/30 border border-white/10 rounded px-2 py-1 text-slate-300 font-mono text-[9px]"
                                  onClick={(e) => (e.target as HTMLInputElement).select()}
                                />
                                <span className="text-slate-500 block text-[9px] leading-tight">Mẹo: Sao chép link này và dán vào phần <strong>Mô tả chi tiết</strong> của dự án dạng Markdown: <code>[Xem tài liệu]({lastUploadedUrl})</code> để thành viên hoặc sinh viên khác có thể click xem!</span>
                              </div>
                            )}
                          </div>
                        )}

                        <div className="relative border-2 border-dashed border-white/10 rounded-lg p-6 hover:border-purple-500/30 hover:bg-white/5 transition-all text-center cursor-pointer">
                          {uploadProgress ? (
                            <div className="text-xs text-purple-400 animate-pulse">Đang truyền tải luồng bytes lên Azure...</div>
                          ) : (
                            <div>
                              <UploadCloud className="w-8 h-8 text-slate-400 mx-auto mb-2" />
                              <span className="text-xs text-slate-300 block">Kéo thả hoặc nhấp chọn để tải lên</span>
                              <input 
                                type="file" 
                                onChange={handleFileUpload}
                                className="absolute inset-0 opacity-0 cursor-pointer"
                              />
                            </div>
                          )}
                        </div>
                      </div>
                    </div>

                    {/* Milestones and Team Placements lists */}
                    <div className="glass-panel p-6 border-white/5 lg:col-span-2 space-y-8">
                      {/* Active Roster List */}
                      <div className="space-y-4">
                        <div>
                          <h3 className="text-lg font-heading font-bold text-slate-200">Đội ngũ thành viên (Team Placements)</h3>
                          <p className="text-xs text-slate-400">Danh sách nhân sự liên khoa chính thức đã được bạn phê duyệt.</p>
                        </div>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                          {founderProject.teamMembers.length === 0 ? (
                            <p className="text-xs text-slate-500 py-2 col-span-2">Dự án chưa có thành viên nào.</p>
                          ) : (
                            founderProject.teamMembers.map(member => (
                              <div key={member.id} className="p-3 rounded-lg bg-white/5 border border-white/5 flex items-center justify-between">
                                <div>
                                  <h4 className="font-heading font-bold text-sm text-slate-200">{member.name}</h4>
                                  <p className="text-[10px] text-slate-400 mt-1">{member.studentId} • {member.email}</p>
                                </div>
                                <div className="flex items-center gap-2">
                                  {member.contactLink && (
                                    <a href={member.contactLink} target="_blank" rel="noreferrer" className="text-slate-400 hover:text-cyan-400 p-1.5 rounded hover:bg-white/5">
                                      <LinkIcon className="w-4 h-4" />
                                    </a>
                                  )}
                                  {member.role !== 'Founder' && (
                                    <button 
                                      onClick={() => handleRemoveMember(member.id)}
                                      className="text-rose-500 hover:text-rose-400 p-1.5 rounded hover:bg-rose-500/10"
                                      title="Loại bỏ thành viên"
                                    >
                                      <Trash2 className="w-4 h-4" />
                                    </button>
                                  )}
                                </div>
                              </div>
                            ))
                          )}
                        </div>
                      </div>

                      {/* Milestones scheduler */}
                      <div className="space-y-4 border-t border-white/5 pt-6">
                        <div>
                          <h3 className="text-lg font-heading font-bold text-slate-200">Lịch trình Cột mốc</h3>
                          <p className="text-xs text-slate-400">Đăng cột mốc mới để gỡ cảnh báo hoặc cập nhật tiến trình của startup.</p>
                        </div>

                        <form onSubmit={handleAddMilestone} className="grid grid-cols-1 md:grid-cols-3 gap-3 items-end">
                          <div className="md:col-span-2">
                            <input 
                              type="text" 
                              placeholder="Tiêu đề cột mốc mới..." 
                              value={newMilestoneTitle}
                              onChange={(e) => setNewMilestoneTitle(e.target.value)}
                              required 
                            />
                          </div>
                          <button type="submit" className="btn btn-secondary py-2.5">
                            <Plus className="w-4 h-4" /> Thêm mới
                          </button>
                        </form>

                        <div className="space-y-3">
                          {founderProject.milestones.map(m => (
                            <div key={m.id} className="p-3.5 rounded-lg bg-white/5 border border-white/5 flex items-center justify-between">
                              <div className="flex items-center gap-3">
                                {m.done ? (
                                  <CheckSquare className="w-5 h-5 text-emerald-400 shrink-0" />
                                ) : (
                                  <div className="w-5 h-5 border-2 border-slate-500 rounded shrink-0"></div>
                                )}
                                <div>
                                  <h4 className={`text-sm font-bold ${m.done ? 'line-through text-slate-500' : 'text-slate-200'}`}>{m.title}</h4>
                                  <p className="text-[10px] text-slate-500 mt-1">Khởi tạo: {new Date(m.createdAt).toLocaleDateString()}</p>
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    </div>
                  </div>
                )}

                {/* TAB: Candidates & Recruits */}
                {activeTab === 'recruits' && (
                  <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Post new job opportunity */}
                    <div className="glass-panel p-6 border-white/5 space-y-6">
                      <div>
                        <h3 className="text-lg font-heading font-bold text-slate-200">Đăng tin tuyển dụng</h3>
                        <p className="text-xs text-slate-400">Tìm kiếm các mảnh ghép sinh viên liên khoa (SE, GD, Biz).</p>
                      </div>

                      <form onSubmit={handlePostJob} className="space-y-4">
                        <div className="space-y-1">
                          <label className="text-xs text-slate-400 font-bold uppercase">Tiêu đề vai trò</label>
                          <input 
                            type="text" 
                            placeholder="Ví dụ: Lập trình viên React..." 
                            value={newJobTitle}
                            onChange={(e) => setNewJobTitle(e.target.value)}
                            required 
                          />
                        </div>

                        <div className="space-y-1">
                          <label className="text-xs text-slate-400 font-bold uppercase">Nhóm chuyên ngành</label>
                          <select 
                            value={newJobCategory}
                            onChange={(e) => setNewJobCategory(e.target.value as any)}
                          >
                            <option value="Engineering">Kỹ thuật (Engineering)</option>
                            <option value="Design">Thiết kế (Design)</option>
                            <option value="Business">Kinh doanh (Business)</option>
                            <option value="Marketing">Truyền thông (Marketing)</option>
                          </select>
                        </div>

                        <div className="space-y-1">
                          <label className="text-xs text-slate-400 font-bold uppercase">Mô tả công việc</label>
                          <textarea 
                            rows={3} 
                            placeholder="Mô tả các nhiệm vụ chính..." 
                            value={newJobDesc}
                            onChange={(e) => setNewJobDesc(e.target.value)}
                            required 
                          />
                        </div>

                        <div className="space-y-1">
                          <label className="text-xs text-slate-400 font-bold uppercase">Yêu cầu kĩ năng</label>
                          <textarea 
                            rows={2} 
                            placeholder="Thành thạo React, Git..." 
                            value={newJobReqs}
                            onChange={(e) => setNewJobReqs(e.target.value)}
                          />
                        </div>

                        <button type="submit" className="btn btn-primary w-full">Đăng tin tuyển dụng</button>
                      </form>
                    </div>

                    {/* Applicant Review Board */}
                    <div className="glass-panel p-6 border-white/5 lg:col-span-2 space-y-6">
                      <div>
                        <h3 className="text-lg font-heading font-bold text-slate-200">Đơn ứng tuyển đang chờ duyệt</h3>
                        <p className="text-xs text-slate-400">Phê duyệt hoặc từ chối sinh viên liên khoa nộp đơn vào dự án.</p>
                      </div>

                      <div className="space-y-4">
                        {candidates.filter(c => c.status === 'Pending').length === 0 ? (
                          <p className="text-sm text-slate-500 text-center py-12">Không có hồ sơ nào đang chờ duyệt.</p>
                        ) : (
                          candidates.filter(c => c.status === 'Pending').map(c => (
                            <div key={c.id} className="p-5 rounded-lg bg-white/5 border border-white/10 flex flex-col md:flex-row justify-between gap-4">
                              <div className="space-y-2">
                                <div className="flex items-center gap-2.5">
                                  <span className="badge badge-draft text-[9px]">{c.job_title}</span>
                                  <span className="text-[10px] text-slate-500">{new Date(c.createdAt).toLocaleDateString()}</span>
                                </div>
                                <h4 className="font-heading font-bold text-base text-slate-100">{c.student_name} ({c.student_id})</h4>
                                <div className="flex flex-wrap gap-4 text-xs text-slate-400">
                                  <span className="flex items-center gap-1"><Mail className="w-3.5 h-3.5" /> {c.student_email}</span>
                                  {c.student_contact && (
                                    <a href={c.student_contact} target="_blank" rel="noreferrer" className="flex items-center gap-1 text-cyan-400 hover:underline">
                                      <LinkIcon className="w-3.5 h-3.5" /> Liên hệ
                                    </a>
                                  )}
                                  {c.student_cv && (
                                    <a href={c.student_cv} target="_blank" rel="noreferrer" className="flex items-center gap-1 text-purple-400 hover:underline">
                                      <FileText className="w-3.5 h-3.5" /> Xem CV (PDF)
                                    </a>
                                  )}
                                </div>
                              </div>

                              <div className="flex items-center gap-2 self-end md:self-center">
                                <button 
                                  onClick={() => handleReviewCandidate(c.id, 'Rejected')}
                                  className="btn btn-outline py-1.5 px-3 text-xs text-rose-400 hover:bg-rose-500/10 border-rose-500/20"
                                >
                                  Từ chối
                                </button>
                                <button 
                                  onClick={() => handleReviewCandidate(c.id, 'Approved')}
                                  className="btn btn-secondary py-1.5 px-3 text-xs"
                                >
                                  Duyệt vào nhóm
                                </button>
                              </div>
                            </div>
                          ))
                        )}
                      </div>
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>
        )}

        {/* ================= MANAGER PORTAL ================= */}
        {currentUser.role === 'Manager' && (
          <div className="space-y-6">
            <div className="tabs-container">
              <button 
                onClick={() => { setActiveTab('vetting-queue'); fetchManagerDashboard(); }} 
                className={`tab-btn ${activeTab === 'vetting-queue' ? 'active' : ''}`}
              >
                Xét duyệt Startup ({projects.filter(p => p.status === 'Draft').length})
              </button>
              <button 
                onClick={() => { setActiveTab('all-projects'); fetchManagerDashboard(); }} 
                className={`tab-btn ${activeTab === 'all-projects' ? 'active' : ''}`}
              >
                Danh sách Vườn Ươm
              </button>
            </div>

            {/* TAB: Startup Vetting Queue */}
            {activeTab === 'vetting-queue' && (
              <div className="space-y-6">
                <div>
                  <h2 className="text-2xl font-heading font-bold text-slate-200">Startup Vetting Queue</h2>
                  <p className="text-sm text-slate-400">Xét duyệt các ý tưởng dự án nộp bản nháp trước khi cho phép xuất hiện trên bảng tuyển dụng.</p>
                </div>

                <div className="space-y-4">
                  {projects.filter(p => p.status === 'Draft').length === 0 ? (
                    <p className="text-sm text-slate-500 text-center py-12">Không có dự án khởi nghiệp nào đang chờ duyệt.</p>
                  ) : (
                    projects.filter(p => p.status === 'Draft').map(p => (
                      <div key={p.id} className="glass-panel p-6 border-white/5 flex flex-col md:flex-row justify-between gap-6">
                        <div className="space-y-3 max-w-3xl">
                          <h3 className="text-xl font-heading font-bold text-slate-100">{p.name}</h3>
                          <p className="text-sm text-slate-300 bg-white/5 p-3 rounded border border-white/5 italic">"{p.pitch}"</p>
                          <div className="flex items-center gap-4 text-xs text-slate-400">
                            <span>Khởi tạo: {new Date(p.lastUpdatedAt).toLocaleDateString()}</span>
                            <span>•</span>
                            <span>{p.milestones.length} Cột mốc</span>
                          </div>
                        </div>
                        <div className="flex items-center gap-2.5 self-end md:self-center">
                          <button 
                            onClick={() => handleVetProject(p.id, 'Active')}
                            className="btn btn-secondary py-2 px-4"
                          >
                            Phê duyệt Đăng
                          </button>
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            )}

            {/* TAB: Portfolio Review & Dormancy Scan */}
            {activeTab === 'all-projects' && (
              <div className="space-y-6">
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                  <div>
                    <h2 className="text-2xl font-heading font-bold text-slate-200">Incubator Portfolio</h2>
                    <p className="text-sm text-slate-400">Giám sát hoạt động, quotas bộ nhớ và tình trạng cập nhật của các startup.</p>
                  </div>

                  <div className="flex items-center gap-3">
                    <button 
                      onClick={handleRunDormancyCheck}
                      className="btn btn-outline py-2.5 px-4 text-xs border-amber-500/20 text-amber-400 hover:bg-amber-500/10 flex items-center gap-2"
                    >
                      <AlertTriangle className="w-4 h-4" /> Quét Dormancy
                    </button>

                    <a 
                      href="/api/admin/reports/csv"
                      className="btn btn-secondary py-2.5 px-4 text-xs flex items-center gap-2"
                    >
                      <FileSpreadsheet className="w-4 h-4" /> Xuất báo cáo CSV
                    </a>
                  </div>
                </div>

                <div className="glass-panel overflow-hidden border-white/5">
                  <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse text-sm text-slate-300">
                      <thead>
                        <tr className="bg-white/5 border-b border-white/5 text-slate-400 font-heading text-xs font-bold uppercase">
                          <th className="p-4">Tên Startup</th>
                          <th className="p-4">Tình trạng</th>
                          <th className="p-4">Dung lượng sử dụng</th>
                          <th className="p-4">Thành viên</th>
                          <th className="p-4">Cập nhật cuối</th>
                          <th className="p-4 text-right">Hành động</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-white/5">
                        {projects.map(p => (
                          <tr key={p.id} className="hover:bg-white/5 transition-colors">
                            <td className="p-4 font-heading font-bold text-slate-200">{p.name}</td>
                            <td className="p-4">
                              <span className={`badge text-[9px] ${
                                p.status === 'Active' ? 'badge-active' :
                                p.status === 'Suspended' ? 'badge-suspended' :
                                p.status === 'At-Risk' ? 'badge-risk' : 'badge-draft'
                              }`}>{p.status}</span>
                            </td>
                            <td className="p-4 font-mono text-xs">{formatBytes(p.storageUsedBytes)}</td>
                            <td className="p-4">{p.teamMembers.length} Nhân sự</td>
                            <td className="p-4 text-xs text-slate-400">{new Date(p.lastUpdatedAt).toLocaleDateString()}</td>
                            <td className="p-4 text-right">
                              {p.status === 'Active' && (
                                <button 
                                  onClick={() => handleVetProject(p.id, 'Suspended')}
                                  className="btn btn-outline py-1 px-2.5 text-xs text-rose-400 border-rose-500/20 hover:bg-rose-500/10"
                                >
                                  Tạm dừng
                                </button>
                              )}
                              {p.status === 'Suspended' && (
                                <button 
                                  onClick={() => handleVetProject(p.id, 'Active')}
                                  className="btn btn-outline py-1 px-2.5 text-xs text-emerald-400 border-emerald-500/20 hover:bg-emerald-500/10"
                                >
                                  Mở lại
                                </button>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            )}
          </div>
        )}
      </main>

      {/* --- SELECTED PROJECT MODAL DIALOG --- */}
      {selectedProject && (
        <div className="fixed inset-0 bg-black/80 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="glass-panel max-w-2xl w-full p-6 relative border-purple-500/30 animate-fade-in max-h-[90vh] overflow-y-auto">
            <button 
              onClick={() => setSelectedProject(null)}
              className="absolute top-4 right-4 text-slate-400 hover:text-white"
            >
              <XCircle className="w-6 h-6" />
            </button>

            <div className="space-y-6">
              <div>
                <span className="badge badge-active mb-2">{selectedProject.status}</span>
                <h2 className="text-2xl font-heading font-extrabold text-slate-100">{selectedProject.name}</h2>
                <p className="text-sm text-slate-400 mt-2 italic">"{selectedProject.pitch}"</p>
              </div>

              {selectedProject.demoUrl && (
                <div className="p-4 rounded-lg bg-cyan-500/5 border border-cyan-500/20 flex items-center justify-between text-left">
                  <div>
                    <h4 className="text-sm font-heading font-bold text-cyan-400">Showcase dự án mẫu trực quan</h4>
                    <p className="text-[11px] text-slate-400 mt-0.5">Dự án này sở hữu bản demo/mockup tương tác để trải nghiệm sản phẩm.</p>
                  </div>
                  <a 
                    href={selectedProject.demoUrl} 
                    target="_blank" 
                    rel="noopener noreferrer" 
                    className="btn btn-primary py-2 px-4 rounded-lg text-xs font-semibold font-heading shrink-0"
                  >
                    Xem Showcase ↗
                  </a>
                </div>
              )}
              {/* Detailed Description (Markdown) */}
              <div className="space-y-2 border-t border-white/5 pt-4 text-left">
                <h3 className="text-sm font-heading font-bold text-slate-300">Mô tả chi tiết</h3>
                <div className="p-4 rounded-lg bg-white/5 border border-white/5 max-h-[250px] overflow-y-auto">
                  {renderMarkdown(selectedProject.description)}
                </div>
              </div>

              {/* Roster of members */}
              <div className="space-y-3">
                <h3 className="text-sm font-heading font-bold text-slate-300 flex items-center gap-2">
                  <Users className="w-4 h-4 text-purple-400" /> Thành viên dự án
                </h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                  {selectedProject.teamMembers.length === 0 ? (
                    <p className="text-xs text-slate-500">Dự án chưa gán thành viên chính thức.</p>
                  ) : (
                    selectedProject.teamMembers.map(m => (
                      <div key={m.id} className="p-3 rounded-lg bg-white/5 border border-white/5 flex items-center justify-between text-xs text-slate-300">
                        <div>
                          <p className="font-bold text-slate-200">{m.name}</p>
                          <p className="text-[10px] text-slate-500 mt-0.5">{m.studentId} • {m.email}</p>
                        </div>
                        {m.contactLink && (
                          <a href={m.contactLink} target="_blank" rel="noreferrer" className="text-slate-400 hover:text-cyan-400">
                            <LinkIcon className="w-3.5 h-3.5" />
                          </a>
                        )}
                      </div>
                    ))
                  )}
                </div>
              </div>

              {/* Milestones lists */}
              <div className="space-y-3">
                <h3 className="text-sm font-heading font-bold text-slate-300 flex items-center gap-2">
                  <CheckCircle className="w-4 h-4 text-cyan-400" /> Cột mốc hoàn thành
                </h3>
                <div className="space-y-2">
                  {selectedProject.milestones.length === 0 ? (
                    <p className="text-xs text-slate-500">Chưa ghi nhận cột mốc nào.</p>
                  ) : (
                    selectedProject.milestones.map(m => (
                      <div key={m.id} className="p-3 rounded-lg bg-white/5 border border-white/5 flex items-center justify-between text-xs">
                        <span className={m.done ? 'line-through text-slate-500' : 'text-slate-300'}>{m.title}</span>
                        {m.done && <span className="text-[10px] text-emerald-400">Hoàn thành</span>}
                      </div>
                    ))
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* --- FOOTER --- */}
      <footer className="mt-auto border-t border-white/5 py-6 text-center text-xs text-slate-500">
        <div>© 2026 Gara Startup Showcase. Built with React + .NET 9 & Azure Flexible PostgreSQL.</div>
      </footer>
    </div>
  );
}
