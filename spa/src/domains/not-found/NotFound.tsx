import { Button } from "@mantine/core";
import { Link, useNavigate } from "react-router-dom";

const NotFound = () => {
    const navigate = useNavigate();

    return (
        <div className="min-h-screen w-full flex flex-col items-center justify-center gap-8 relative overflow-hidden">
            {/* Floating background elements */}
            <div className="absolute top-20 left-20 w-64 h-64 bg-purple-100 rounded-full blur-3xl opacity-50 animate-pulse" />
            <div className="absolute bottom-20 right-20 w-80 h-80 bg-purple-200 rounded-full blur-3xl opacity-30 animate-pulse" />
            <div className="absolute top-1/3 right-1/4 w-40 h-40 bg-blue-100 rounded-full blur-3xl opacity-40 animate-pulse" />

            {/* Content */}
            <div className="relative z-10 flex flex-col items-center gap-6">
                <div className="relative">
                    <div className="text-[10rem] font-black leading-none text-transparent bg-clip-text bg-gradient-to-br from-purple-400 to-purple-700 select-none">
                        404
                    </div>
                    <div className="absolute inset-0 text-[10rem] font-black leading-none text-transparent bg-clip-text bg-gradient-to-br from-purple-400 to-purple-700 blur-2xl opacity-30 select-none">
                        404
                    </div>
                </div>

                <div className="text-2xl font-semibold text-gray-800">
                    Houston, we lost our momentum
                </div>
                <p className="text-gray-400 text-center max-w-sm">
                    This page floated off into space. We've checked the backlog
                    and it's definitely not assigned to anyone.
                </p>

                <div className="flex gap-3 mt-2">
                    <Button
                        variant="outline"
                        color="purple"
                        size="md"
                        onClick={() => navigate(-1)}
                    >
                        Retrace My Steps
                    </Button>
                    <Button
                        color="purple"
                        size="md"
                        component={Link}
                        to='/'
                    >
                        Dashboard
                    </Button>
                </div>

                <p className="text-xs text-gray-300 mt-8">
                    Error 404 &middot; Page not found &middot; Probably never will be
                </p>
            </div>
        </div>
    );
};

export default NotFound;
