import { RouterProvider } from "react-router-dom"
import { Routes } from "./config/routes"

const App = () => {
  return (
    <RouterProvider router={Routes} />
  )
}

export default App
