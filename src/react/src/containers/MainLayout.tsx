import { ChevronRightIcon } from '@chakra-ui/icons';
import { IconButton, useBreakpointValue } from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { Route, Switch } from 'react-router-dom';
import ServerOverview from '../pages/servers/ServerOverview';
import './MainLayout.scss';
import NavLayout from './NavLayout';

const smVariant = { navigation: 'drawer', navigationButton: true };
const mdVariant = { navigation: 'sidebar', navigationButton: false };

const MainLayout: FC = () => {
    const [isSidebarOpen, setSidebarOpen] = useState(false);
    const variants = useBreakpointValue({ base: smVariant, md: mdVariant });
    const toggleSidebar = () => setSidebarOpen(!isSidebarOpen);

    return (
        <>
            {variants?.navigationButton ? (
                <IconButton
                    icon={<ChevronRightIcon w={8} h={8} />}
                    colorScheme="blackAlpha"
                    variant="outline"
                    onClick={() => toggleSidebar()}
                    aria-label=""
                />
            ) : null}
            <NavLayout
                variant={variants?.navigation}
                isOpen={isSidebarOpen}
                onClose={toggleSidebar}
            ></NavLayout>

            <Switch>
                <Route
                    path="/server/:id"
                    render={(props) => <ServerOverview {...props} />}
                />
            </Switch>
        </>
    );
};

export default MainLayout;
