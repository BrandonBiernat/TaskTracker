import { Outlet } from "react-router-dom";
import { AuthProvider } from "../config";

const Root = () => {
    return (
        <AuthProvider>
            <div className='min-h-screen w-full animate-fade-in bg-[#f8f7fc]'>
                <Outlet />
            </div>
        </AuthProvider>
    );
}

export default Root;