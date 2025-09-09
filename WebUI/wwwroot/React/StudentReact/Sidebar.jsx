export default function Sidebar() {
    return (
        <aside className="sidebar">
            <div className="user">
                <div>👤</div>
                <div><strong>Ahmet Yılmaz</strong><br /><small>Bilgisayar Müh.</small></div>
            </div>
            <nav>
                <button className="active">Ana Sayfa</button>
                <button>Profil</button>
                <button>Belgelerim</button>
                <button>Başvurularım</button>
                <button>Staj Defteri</button>
                <button>Takvim</button>
                <button>İstatistikler</button>
                <button>Yardım</button>
            </nav>
        </aside>
    );
}
